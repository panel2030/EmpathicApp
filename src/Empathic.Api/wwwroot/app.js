const api = async (url, options = {}) => {
  const response = await fetch(url, { headers: { 'Content-Type': 'application/json', ...(options.headers || {}) }, ...options });
  const text = await response.text();
  const body = text ? JSON.parse(text) : null;
  if (!response.ok) throw new Error(body?.message || `Request failed (${response.status})`);
  return body;
};

const esc = value => String(value ?? '').replace(/[&<>'"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[c]));

async function loadDashboard(){
  const d = await api('/api/dashboard');
  document.getElementById('stats').innerHTML = [
    ['Creators', d.creators], ['Registered works', d.works], ['Anchored', d.anchoredWorks], ['Pending anchoring', d.pendingAnchoring]
  ].map(([label,value]) => `<div class="stat"><b>${value}</b><span>${label}</span></div>`).join('');
}

async function loadCreators(){
  const creators = await api('/api/creators');
  const select = document.getElementById('creatorSelect');
  select.innerHTML = creators.length
    ? creators.map(c => `<option value="${c.id}">${esc(c.displayName)}${c.institution ? ' — '+esc(c.institution) : ''}</option>`).join('')
    : '<option value="">Register a creator first</option>';
}

async function loadWorks(){
  const works = await api('/api/works');
  const creators = await api('/api/creators');
  const names = Object.fromEntries(creators.map(c => [c.id, c.displayName]));
  document.getElementById('works').innerHTML = works.length ? works.slice().reverse().map(w => `
    <div class="work">
      <div>
        <span class="status ${w.provenanceStatus === 'anchored' ? '' : 'pending'}">${esc(w.provenanceStatus)}</span>
        <h3>${esc(w.title)}</h3>
        <div class="meta">${esc(w.workType)} • ${esc(names[w.creatorId] || w.creatorId)} • ${new Date(w.createdAtUtc).toLocaleString()}</div>
        <div class="hash"><strong>SHA-256</strong><br>${esc(w.contentHash)}</div>
        ${w.transactionHash ? `<div class="hash"><strong>${esc(w.blockchainNetwork)}</strong><br>${esc(w.transactionHash)}</div>` : ''}
      </div>
      <div>${w.transactionHash ? '' : `<button onclick="anchorWork('${w.id}')">Anchor</button>`}</div>
    </div>`).join('') : '<p>No works registered yet.</p>';
}

async function anchorWork(id){
  try { await api(`/api/works/${id}/anchor`, { method:'POST' }); await refresh(); }
  catch(e){ alert(e.message); }
}

async function verifyHash(){
  const hash = document.getElementById('verifyHash').value.trim();
  const out = document.getElementById('verifyResult');
  if(!hash){ out.textContent='Enter a hash.'; return; }
  try { out.textContent = JSON.stringify(await api(`/api/verify/${encodeURIComponent(hash)}`), null, 2); }
  catch(e){ out.textContent = `Not verified: ${e.message}`; }
}

document.getElementById('creatorForm').addEventListener('submit', async e => {
  e.preventDefault(); const f = new FormData(e.target);
  try {
    await api('/api/creators', { method:'POST', body:JSON.stringify({
      displayName:f.get('displayName'), institution:f.get('institution'), country:f.get('country'), website:null,
      roles:String(f.get('roles') || '').split(',').map(x=>x.trim()).filter(Boolean)
    })});
    e.target.reset(); await refresh();
  } catch(err){ alert(err.message); }
});

document.getElementById('workForm').addEventListener('submit', async e => {
  e.preventDefault(); const f = new FormData(e.target);
  try {
    await api('/api/works', { method:'POST', body:JSON.stringify({
      creatorId:f.get('creatorId'), title:f.get('title'), workType:f.get('workType'), description:f.get('description'),
      externalUri:f.get('externalUri'), rightsStatement:'All rights reserved', contentFingerprintSource:f.get('contentFingerprintSource')
    })});
    e.target.reset(); await refresh();
  } catch(err){ alert(err.message); }
});

async function refresh(){ await Promise.all([loadDashboard(), loadCreators(), loadWorks()]); }
refresh().catch(err => console.error(err));
