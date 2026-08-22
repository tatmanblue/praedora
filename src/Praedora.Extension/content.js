// Injected on demand by popup.js via chrome.scripting.executeScript — not a persistent content
// script. Prefers schema.org JobPosting JSON-LD (present on most job boards: LinkedIn, Indeed,
// Greenhouse, Lever) and falls back to page title/text when it's absent.
(() => {
  function findJobPostingJsonLd() {
    const scripts = document.querySelectorAll('script[type="application/ld+json"]');
    for (const script of scripts) {
      try {
        const data = JSON.parse(script.textContent);
        const items = Array.isArray(data) ? data : [data];
        for (const item of items) {
          if (item['@type'] === 'JobPosting') {
            return item;
          }
        }
      } catch {
        // Malformed JSON-LD on the page — ignore and keep looking.
      }
    }
    return null;
  }

  function htmlToText(html) {
    return new DOMParser().parseFromString(html, 'text/html').body.textContent.trim();
  }

  const jobPosting = findJobPostingJsonLd();

  const companyName = jobPosting?.hiringOrganization?.name ?? '';
  const roleTitle = jobPosting?.title ?? document.title ?? '';
  const jobDescriptionRaw = jobPosting?.description
    ? htmlToText(jobPosting.description)
    : (document.body?.innerText ?? '').trim();

  return {
    companyName,
    roleTitle,
    jobDescriptionRaw,
    sourceUrl: window.location.href
  };
})();
