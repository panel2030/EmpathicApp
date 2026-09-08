(() => {
  const menuToggle = document.getElementById('menuToggle');
  const siteNav = document.getElementById('siteNav');

  if (menuToggle && siteNav) {
    menuToggle.addEventListener('click', () => {
      const isOpen = siteNav.classList.toggle('open');
      menuToggle.setAttribute('aria-expanded', String(isOpen));
    });

    siteNav.querySelectorAll('a').forEach(link => link.addEventListener('click', () => {
      siteNav.classList.remove('open');
      menuToggle.setAttribute('aria-expanded', 'false');
    }));
  }

  const year = document.getElementById('year');
  if (year) year.textContent = new Date().getFullYear();

  const revealItems = document.querySelectorAll('.reveal');
  if ('IntersectionObserver' in window) {
    const observer = new IntersectionObserver(entries => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible');
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.12 });
    revealItems.forEach(item => observer.observe(item));
  } else {
    revealItems.forEach(item => item.classList.add('visible'));
  }

  const setText = (id, value) => {
    const el = document.getElementById(id);
    if (el) el.textContent = value;
  };

  async function loadNetworkStatus() {
    const status = document.getElementById('networkStatus');
    try {
      const [healthResponse, dashboardResponse] = await Promise.all([
        fetch('/health', { headers: { Accept: 'application/json' } }),
        fetch('/api/dashboard', { headers: { Accept: 'application/json' } })
      ]);

      if (!healthResponse.ok || !dashboardResponse.ok) throw new Error('API unavailable');
      const dashboard = await dashboardResponse.json();
      setText('statCreators', dashboard.creators ?? 0);
      setText('statWorks', dashboard.works ?? 0);
      setText('statAnchored', dashboard.anchoredWorks ?? 0);
      if (status) status.textContent = 'Prototype network online';
    } catch {
      setText('statCreators', '0');
      setText('statWorks', '0');
      setText('statAnchored', '0');
      if (status) status.textContent = 'Public website ready';
    }
  }

  loadNetworkStatus();
})();
