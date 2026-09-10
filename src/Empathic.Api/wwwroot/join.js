(() => {
  const form = document.getElementById('joinForm');
  const result = document.getElementById('joinResult');
  const year = document.getElementById('year');
  if (year) year.textContent = new Date().getFullYear();
  if (!form || !result) return;

  const setResult = (message, state) => {
    result.textContent = message;
    result.className = `join-result ${state}`;
  };

  form.addEventListener('submit', async event => {
    event.preventDefault();
    if (!form.reportValidity()) return;

    const data = new FormData(form);
    const payload = {
      displayName: String(data.get('displayName') || '').trim(),
      email: String(data.get('email') || '').trim(),
      country: String(data.get('country') || '').trim() || null,
      interestType: String(data.get('interestType') || 'Supporter'),
      message: String(data.get('message') || '').trim() || null,
      consentToUpdates: data.get('consent') === 'on'
    };

    const button = form.querySelector('button[type="submit"]');
    if (button) button.disabled = true;
    setResult('Registering your interest…', 'loading');

    try {
      const response = await fetch('/api/movement/join', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(payload)
      });
      const body = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(body.message || 'We could not register your interest.');
      form.reset();
      setResult(body.message || 'Welcome to the Empathic Movement. Your interest has been registered.', 'success');
    } catch (error) {
      setResult(error.message || 'Something went wrong. Please try again.', 'error');
    } finally {
      if (button) button.disabled = false;
    }
  });
})();
