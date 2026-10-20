"""Testes com SQL Server isolado e API real. --browser inclui testes Chromium."""
import os, pathlib, subprocess, sys, time, urllib.request, uuid
from dev import ROOT, environment

env = environment(docker=True, seed=True, database='NexaCoreTests_' + uuid.uuid4().hex)
env['ASPNETCORE_URLS'] = 'http://127.0.0.1:5081'
env['NEXACORE_TEST_URL'] = env['ASPNETCORE_URLS']
env['NEXACORE_TEST_CONNECTION'] = env['ConnectionStrings__DefaultConnection']
subprocess.run(['dotnet', 'build', str(ROOT/'backend/PIM3SemBackend.sln')], env=env, check=True)

def stop(api):
    api.terminate()
    try: api.wait(timeout=15)
    except subprocess.TimeoutExpired: api.kill();api.wait()

def start(log):
    process = subprocess.Popen(['dotnet', 'run', '--no-build', '--no-launch-profile', '--project', str(ROOT/'backend/PIM3SemBackend')], env=env, cwd=ROOT, stdout=log, stderr=log)
    try:
        for _ in range(120):
            if process.poll() is not None: raise RuntimeError('API encerrou; consulte .local/test-api.log.')
            try:
                with urllib.request.urlopen(env['ASPNETCORE_URLS']+'/api/health', timeout=2) as response:
                    if response.status == 200: return process
            except Exception: time.sleep(1)
        raise RuntimeError('API não ficou pronta; consulte .local/test-api.log.')
    except Exception:
        stop(process);raise

with (ROOT/'.local/test-api.log').open('w') as log:
    api = start(log)
    try:
        result = subprocess.run(['dotnet', 'test', str(ROOT/'backend/PIM3SemBackend.sln'), '--no-build', '--logger', 'trx', '--results-directory', str(ROOT/'.local/TestResults')], env=env)
        if result.returncode == 0 and '--browser' in sys.argv:
            # Ciclo independente de serviço para os testes UI; também comprova seed idempotente.
            # Evita consumir a janela de login da UI com autenticações dos testes API.
            stop(api);api = start(log)
            browser_env = env.copy()
            if pathlib.Path('/usr/bin/chromium').exists(): browser_env['CHROMIUM_PATH'] = '/usr/bin/chromium'
            result = subprocess.run(['npm.cmd' if os.name == 'nt' else 'npm', 'test'], cwd=ROOT/'tests/browser', env=browser_env)
        sys.exit(result.returncode)
    finally:
        stop(api)
        print('Banco de testes isolado retido para diagnóstico. Logs e resultados em .local/.')
