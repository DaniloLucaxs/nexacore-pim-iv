"""Execução local sem credenciais versionadas. Python 3.10+, .NET 8; Docker opcional."""
import argparse, json, os, pathlib, secrets, shutil, subprocess, time

ROOT = pathlib.Path(__file__).resolve().parents[1]
IMAGE = 'mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04@sha256:ea73825f3d88a23c355ac2f9fdc6bd960fec90171c12c572109b36a558f77bb8'

def environment(docker=False, seed=False, database='NexaCore'):
    env = os.environ.copy()
    local = ROOT / '.local'
    local.mkdir(exist_ok=True)
    cloud_sdk = pathlib.Path('/workspace/.tools/dotnet')
    if (cloud_sdk / 'dotnet').exists():
        env.update(DOTNET_ROOT=str(cloud_sdk), DOTNET_CLI_HOME='/workspace/.tools/dotnet-home', NUGET_PACKAGES='/workspace/.tools/nuget', TMPDIR='/workspace/.tools/tmp')
        pathlib.Path(env['TMPDIR']).mkdir(parents=True, exist_ok=True)
        env['PATH'] = str(cloud_sdk) + os.pathsep + env['PATH']
    if docker:
        path = local / 'runtime.json'
        if path.exists():
            settings = json.loads(path.read_text())
        else:
            settings = {'sql_password': secrets.token_urlsafe(30) + '!aA1', 'demo_password': env.get('Demo__Password') or secrets.token_urlsafe(18) + '!aA1'}
            path.write_text(json.dumps(settings))
            if os.name != 'nt': path.chmod(0o600)
        sql_env = local / 'sql.env'
        sql_env.write_text('ACCEPT_EULA=Y\nMSSQL_PID=Developer\nMSSQL_MEMORY_LIMIT_MB=2048\nMSSQL_SA_PASSWORD=' + settings['sql_password'] + '\n')
        if os.name != 'nt': sql_env.chmod(0o600)
        container = subprocess.run(['docker', 'inspect', 'nexacore-sql'], capture_output=True, text=True)
        if container.returncode:
            subprocess.run(['docker', 'run', '-d', '--name', 'nexacore-sql', '--env-file', str(sql_env), '-p', '127.0.0.1:1433:1433', '-v', 'nexacore-sql-data:/var/opt/mssql', IMAGE], check=True, stdout=subprocess.DEVNULL)
        else:
            subprocess.run(['docker', 'start', 'nexacore-sql'], check=True, stdout=subprocess.DEVNULL)
        check = 'SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -Q "SELECT 1" -b -o /dev/null'
        for _ in range(90):
            if subprocess.run(['docker', 'exec', 'nexacore-sql', 'bash', '-c', check], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL).returncode == 0:
                break
            time.sleep(1)
        else:
            raise RuntimeError('SQL Server não ficou pronto. Consulte docker logs nexacore-sql (sem divulgar credenciais).')
        env['ConnectionStrings__DefaultConnection'] = f"Server=127.0.0.1,1433;Database={database};User Id=sa;Password={settings['sql_password']};Encrypt=true;TrustServerCertificate=true"
        env.setdefault('Demo__Password', settings['demo_password'])
    if not env.get('ConnectionStrings__DefaultConnection'):
        raise RuntimeError('Defina ConnectionStrings__DefaultConnection ou use --docker. Consulte README.md.')
    if seed and not env.get('Demo__Password'):
        raise RuntimeError('Defina Demo__Password (10+ caracteres) para criar usuários demonstrativos.')
    env['Demo__Seed'] = str(seed).lower()
    env.setdefault('ASPNETCORE_ENVIRONMENT', 'Development')
    env.setdefault('ASPNETCORE_URLS', 'http://127.0.0.1:5080')
    return env

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--docker', action='store_true', help='Usar SQL Server Developer em Docker com volume persistente')
    parser.add_argument('--seed', action='store_true', help='Criar dados fictícios (somente demonstração local)')
    args = parser.parse_args()
    env = environment(args.docker, args.seed)
    sdk = shutil.which('dotnet', path=env['PATH'])
    if not sdk: raise SystemExit('Instale o SDK .NET 8.')
    print('Iniciando NexaCore; frontend e API na porta 5080. Ctrl+C encerra a API.', flush=True)
    try:
        subprocess.run([sdk, 'run', '--no-launch-profile', '--project', str(ROOT / 'backend/PIM3SemBackend')], env=env, cwd=ROOT, check=True)
    except KeyboardInterrupt:
        pass
