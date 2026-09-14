(() => {
  const setText = (id, value) => {
    const element = document.getElementById(id);
    if (element) element.textContent = value;
  };

  const clampPercent = value => {
    const number = Number(value);
    if (!Number.isFinite(number)) return 0;
    return Math.min(100, Math.max(0, number));
  };

  const shortAddress = value => {
    if (!value || value === 'Not configured') return 'Not configured';
    if (value.length <= 22) return value;
    return `${value.slice(0, 10)}…${value.slice(-8)}`;
  };

  async function loadSupporters() {
    try {
      const response = await fetch('/api/movement/stats', { headers: { Accept: 'application/json' } });
      if (!response.ok) throw new Error('stats unavailable');
      const data = await response.json();
      setText('supporterCount', data.members ?? data.total ?? 0);
    } catch {
      setText('supporterCount', '0');
    }
  }

  async function loadLaunchConfig() {
    const message = document.getElementById('allocationMessage');
    try {
      const response = await fetch('/api/token/launch-config', { headers: { Accept: 'application/json' } });
      if (!response.ok) throw new Error('configuration unavailable');
      const data = await response.json();
      const founders = Array.isArray(data.founderAllocations) ? data.founderAllocations : [];
      const first = founders[0] ?? {};
      const second = founders[1] ?? {};
      const firstPercent = clampPercent(first.percent ?? 50);
      const secondPercent = clampPercent(second.percent ?? 50);

      setText('launchStatus', data.saleEnabled ? 'Offering enabled after review' : (data.statusLabel || 'Pre-launch · no public sale active'));
      setText('founderOneLabel', first.label || 'Founder 1');
      setText('founderTwoLabel', second.label || 'Founder 2');
      setText('founderOneShare', `${firstPercent}%`);
      setText('founderTwoShare', `${secondPercent}%`);
      setText('founderOnePercent', `${firstPercent}%`);
      setText('founderTwoPercent', `${secondPercent}%`);
      setText('founderOneWallet', first.wallet || 'Not configured');
      setText('founderTwoWallet', second.wallet || 'Not configured');

      const firstBar = document.getElementById('founderOneBar');
      const secondBar = document.getElementById('founderTwoBar');
      if (firstBar) firstBar.style.width = `${firstPercent}%`;
      if (secondBar) secondBar.style.width = `${secondPercent}%`;

      const total = firstPercent + secondPercent;
      if (message) {
        message.textContent = total === 100
          ? `Founder allocation total: ${total}%. Configuration is valid.`
          : `Founder allocation total is ${total}%. Set the two configured percentages to exactly 100% before production use.`;
      }

      document.querySelectorAll('[data-copy]').forEach(button => {
        const targetId = button.getAttribute('data-copy');
        const target = targetId ? document.getElementById(targetId) : null;
        const raw = target?.textContent?.trim() || '';
        button.disabled = !raw || raw === 'Not configured';
        button.title = raw && raw !== 'Not configured' ? `Copy ${shortAddress(raw)}` : 'Configure a public wallet address first';
      });
    } catch {
      setText('launchStatus', 'Pre-launch · no public sale active');
      if (message) message.textContent = 'Using the default 50/50 preview. Configure public founder wallet addresses through server environment variables.';
    }
  }

  document.addEventListener('click', async event => {
    const button = event.target.closest('[data-copy]');
    if (!button || button.disabled) return;
    const target = document.getElementById(button.getAttribute('data-copy'));
    const value = target?.textContent?.trim();
    if (!value || value === 'Not configured') return;
    try {
      await navigator.clipboard.writeText(value);
      const previous = button.textContent;
      button.textContent = 'Copied';
      setTimeout(() => { button.textContent = previous; }, 1400);
    } catch {
      button.textContent = 'Copy unavailable';
    }
  });

  Promise.all([loadSupporters(), loadLaunchConfig()]).catch(() => {});
})();
