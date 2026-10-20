const { test, expect } = require('@playwright/test');
const password = process.env.Demo__Password;
if (!password) throw new Error('Execute via scripts/test.py --browser ou defina Demo__Password.');
async function login(page, email) {
  await page.goto('/login.html');
  await page.getByLabel('E-mail', { exact: true }).fill(email);
  await page.getByLabel('Senha', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/dashboard.html/);
  await expect(page.locator('#loading')).toBeHidden();
  await expect(page.locator('#erro')).toBeEmpty();
}
async function logout(page) {
  if (!(await page.locator('#sair').isVisible())) await page.getByRole('button', { name: 'Menu', exact: true }).click();
  await page.locator('#sair').click();
  await expect(page).toHaveURL(/login.html/);
}
test('login, responsividade, menu móvel e páginas integradas', async ({ page }) => {
  const errors = []; page.on('pageerror', error => errors.push(error.message));
  await login(page, 'rh@nexacore.example');
  for (const width of [390, 768, 1440]) {
    await page.setViewportSize({ width, height: 900 });
    for (const file of ['dashboard', 'funcionarios', 'reajustes', 'auditoria']) {
      await page.goto('/' + file + '.html');
      await expect(page.locator('#loading')).toBeHidden();
      await expect(page.locator('#erro')).toBeEmpty();
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `${file}:${width}`).toBeTruthy();
    }
  }
  await page.setViewportSize({ width: 390, height: 844 });
  await page.getByRole('button', { name: 'Menu', exact: true }).click();
  await expect(page.locator('#side')).toBeVisible();
  await page.keyboard.press('Escape');
  await expect(page.locator('#side')).toBeHidden();
  await page.goto('/dashboard.html');await expect(page.locator('#loading')).toBeHidden();
  await page.screenshot({ path: '../../.local/dashboard-mobile.png', fullPage: true });
  expect(errors).toEqual([]);
});
test('cadastro RH → solicitação gestor → aprovação RH → auditoria SQL', async ({ page }) => {
  await login(page, 'rh@nexacore.example');
  const name = 'Pessoa UI ' + Date.now();
  await page.goto('/funcionarios.html');await expect(page.locator('#loading')).toBeHidden();
  await page.getByRole('button', { name: 'Novo colaborador', exact: true }).click();
  await page.locator('#cadastro').getByLabel('Nome', { exact: true }).fill(name);
  await page.getByLabel('E-mail fictício').fill('ui' + Date.now() + '@example.test');
  await page.getByLabel('Senha inicial').fill('TesteUIFicticio!2026');
  await page.locator('#cadastro').getByLabel('Cargo', { exact: true }).fill('Analista UI');
  await page.getByLabel('Salário inicial').fill('5000');
  await page.locator('#departamento').selectOption({ label: 'Comercial' });
  await page.locator('#perfil').selectOption({ label: 'Colaborador' });
  await page.locator('#gerente').selectOption({ label: 'Marina Duarte' });
  await page.getByRole('button', { name: 'Cadastrar', exact: true }).click();
  await expect(page.locator('#cadastro')).not.toBeVisible();
  await logout(page);
  await login(page, 'gestor@nexacore.example');
  await page.goto('/reajustes.html');await expect(page.locator('#loading')).toBeHidden();
  await page.getByLabel('Colaborador da sua equipe').selectOption({ label: name });
  await page.getByLabel('Aumento', { exact: true }).fill('10');
  await page.getByLabel('Motivo', { exact: true }).fill('Mérito validado no navegador');
  await page.getByRole('button', { name: 'Simular no servidor' }).click();
  await expect(page.locator('#previa')).toContainText('5.500,00');
  await page.getByRole('button', { name: 'Enviar solicitação' }).click();
  await expect(page.locator('#rows')).toContainText(name);
  await logout(page);
  await login(page, 'rh@nexacore.example');
  await page.goto('/reajustes.html');await expect(page.locator('#loading')).toBeHidden();
  const row = page.getByRole('row').filter({ hasText: name });
  await row.getByRole('button', { name: 'Aprovar', exact: true }).click();
  await page.getByRole('button', { name: 'Aprovar reajuste', exact: true }).click();
  await expect(row).toContainText('Aprovada');
  await page.goto('/auditoria.html');await expect(page.locator('#loading')).toBeHidden();
  await page.getByLabel('Colaborador ou responsável').fill(name);
  await page.getByRole('button', { name: 'Filtrar', exact: true }).click();
  await expect(page.locator('#log')).toContainText('Salário');
  await expect(page.locator('#log')).toContainText('rh@nexacore.example');
  await expect(page.locator('#erro')).toBeEmpty();
});
