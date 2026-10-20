const { defineConfig } = require('@playwright/test');
module.exports = defineConfig({
  testDir: '.', testMatch: '*.spec.js', workers: 1, timeout: 45000,
  outputDir: '../../.local/browser-results',
  use: { baseURL: process.env.NEXACORE_TEST_URL || 'http://127.0.0.1:5080',
    headless: true, trace: 'off', screenshot: 'off',
    launchOptions: { ...(process.env.CHROMIUM_PATH ? { executablePath: process.env.CHROMIUM_PATH } : {}) }
  }
});
