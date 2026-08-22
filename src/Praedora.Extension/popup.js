const DEFAULT_API_BASE_URL = 'http://localhost:5080';

async function getApiBaseUrl() {
  const stored = await chrome.storage.sync.get('apiBaseUrl');
  return stored.apiBaseUrl || DEFAULT_API_BASE_URL;
}

async function extractFromActiveTab() {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  const [{ result }] = await chrome.scripting.executeScript({
    target: { tabId: tab.id },
    files: ['content.js']
  });
  return result;
}

function setStatus(message) {
  document.getElementById('status').textContent = message;
}

document.addEventListener('DOMContentLoaded', async () => {
  try {
    const extracted = await extractFromActiveTab();
    document.getElementById('companyName').value = extracted.companyName;
    document.getElementById('roleTitle').value = extracted.roleTitle;
    document.getElementById('sourceUrl').value = extracted.sourceUrl;
    document.getElementById('jobDescriptionRaw').value = extracted.jobDescriptionRaw;
  } catch (err) {
    setStatus(`Could not read this page: ${err.message}`);
  }

  document.getElementById('capture-form').addEventListener('submit', async (event) => {
    event.preventDefault();
    setStatus('Capturing...');

    const payload = {
      companyName: document.getElementById('companyName').value,
      roleTitle: document.getElementById('roleTitle').value,
      jobDescriptionRaw: document.getElementById('jobDescriptionRaw').value,
      sourceUrl: document.getElementById('sourceUrl').value
    };

    try {
      const apiBaseUrl = await getApiBaseUrl();
      const response = await fetch(`${apiBaseUrl}/api/capture`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (!response.ok) {
        throw new Error(`API returned ${response.status}`);
      }

      setStatus('Captured. It is now on the board as "Captured".');
    } catch (err) {
      setStatus(`Capture failed: ${err.message}`);
    }
  });
});
