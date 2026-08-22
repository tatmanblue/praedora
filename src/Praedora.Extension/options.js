const DEFAULT_API_BASE_URL = 'http://localhost:5080';

document.addEventListener('DOMContentLoaded', async () => {
  const stored = await chrome.storage.sync.get('apiBaseUrl');
  document.getElementById('apiBaseUrl').value = stored.apiBaseUrl || DEFAULT_API_BASE_URL;
});

document.getElementById('save').addEventListener('click', async () => {
  const apiBaseUrl = document.getElementById('apiBaseUrl').value.trim() || DEFAULT_API_BASE_URL;
  await chrome.storage.sync.set({ apiBaseUrl });
  document.getElementById('status').textContent = 'Saved.';
});
