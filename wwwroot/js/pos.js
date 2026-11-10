(() => {
    'use strict';
    const $ = id => document.getElementById(id), money = n => n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
    let cart = new Map(), mode = 'Retirada', payment = 'Pix', customer = null, key = crypto.randomUUID(), busy = false, timer, request = 0;
    const products = new Map([...document.querySelectorAll('.add')].map(b => [Number(b.dataset.id), { id: Number(b.dataset.id), name: b.dataset.name, price: Number(b.dataset.price), category: b.dataset.category, stock: Number(b.dataset.stock) }]));
    function el(tag, cls, text) { const e = document.createElement(tag); if (cls) e.className = cls; if (text !== undefined) e.textContent = text; return e; }
    function render() {
        const target = $('cart-items'); target.replaceChildren(); let sub = 0;
        if (!cart.size) { const empty = el('div', 'empty-cart'); empty.append(el('span', '', '▧'), el('h3', '', 'Sua comanda está vazia'), el('p', '', 'Adicione os sabores do pedido.')); target.append(empty); }
        for (const [id, qty] of cart) {
            const p = products.get(id); sub += p.price * qty; const row = el('div', 'cart-row'), name = el('div', 'item-name'); name.append(el('strong', '', p.name), el('small', '', p.category)); const controls = el('div', 'quantity');
            for (const [label, delta] of [['−', -1], ['+', 1]]) { const b = el('button', '', label); b.type = 'button'; b.setAttribute('aria-label', `${delta === 1 ? 'Aumentar' : 'Diminuir'} ${p.name}`); b.addEventListener('click', () => change(id, delta)); if (delta === 1) { controls.append(el('span', '', String(qty))); } controls.append(b); }
            const remove = el('button', 'remove'); remove.innerHTML = '<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="M3 6h18M9 6V3h6v3M5 6l1 15h12l1-15M10 10v7M14 10v7"/></svg>'; remove.type = 'button'; remove.setAttribute('aria-label', `Remover ${p.name}`); remove.onclick = () => { if (busy) return; cart.delete(id); render(); }; row.append(name, controls, el('strong', 'line-price', money(p.price * qty)), remove); target.append(row);
        }
        const fee = mode === 'Delivery' ? 5 : 0; $('subtotal').textContent = money(sub); $('fee').textContent = money(fee); $('total').textContent = money(sub + fee); $('finish').textContent = busy ? 'Registrando…' : `Finalizar Pedido (${money(sub + fee)})`; $('finish').disabled = busy || !cart.size; $('delivery-line').hidden = mode !== 'Delivery';
    }
    function change(id, delta) { if (busy) return; const qty = (cart.get(id) || 0) + delta, p = products.get(id); if (qty > Math.min(p.stock, 999)) { error('Quantidade acima do estoque disponível.'); return; } error(''); if (qty <= 0) cart.delete(id); else cart.set(id, qty); render(); }
    function error(msg) { $('cart-error').textContent = msg; }
    document.querySelectorAll('.add').forEach(b => b.addEventListener('click', () => change(Number(b.dataset.id), 1)));
    let category = 'Todos';
    const normalize = s => s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim();
    function filterProducts() { const text = normalize($('product-search').value); let count = 0; document.querySelectorAll('.product').forEach(p => { p.hidden = (category !== 'Todos' && p.dataset.category !== category) || !normalize(p.dataset.name).includes(text); if (!p.hidden) count++; }); $('product-count').textContent = `${count} ${count === 1 ? 'opção' : 'opções'}`; $('no-products').hidden = count !== 0; }
    $('categories').addEventListener('click', e => { const b = e.target.closest('[data-category]'); if (!b) return; category = b.dataset.category; document.querySelectorAll('#categories button').forEach(x => x.classList.toggle('selected', x === b)); filterProducts(); });
    $('product-search').addEventListener('input', filterProducts);
    $('modes').addEventListener('click', e => { const b = e.target.closest('[data-mode]'); if (!b || busy) return; mode = b.dataset.mode; document.querySelectorAll('#modes button').forEach(x => x.classList.toggle('selected', x === b)); $('address-block').hidden = mode !== 'Delivery'; $('table-block').hidden = mode !== 'Mesa'; $('mode-label').textContent = `Canal: ${mode}`; error(''); render(); });
    $('payments').addEventListener('click', e => { const b = e.target.closest('[data-payment]'); if (!b || busy) return; payment = b.dataset.payment; document.querySelectorAll('#payments button').forEach(x => x.classList.toggle('selected', x === b)); });
    function selectCustomer(c) { customer = c; $('customer-name').textContent = c ? c.nome : 'Consumidor no balcão'; $('address').replaceChildren(); const placeholder = el('option', '', 'Selecione um endereço'); placeholder.value = ''; $('address').append(placeholder); if (c) { for (const a of c.enderecos) { const o = el('option', '', `${a.logradouro}, ${a.numero} — ${a.bairro}`); o.value = a.id; $('address').append(o); } if (c.enderecos.length === 1) $('address').value = c.enderecos[0].id; } $('matches').replaceChildren(); }
    $('clear-customer').onclick = () => { if (busy) return; request++; selectCustomer(null); $('phone').value = ''; };
    $('phone').addEventListener('input', () => { clearTimeout(timer); const seq = ++request; $('matches').replaceChildren(); const v = $('phone').value.replace(/\D/g, ''); if (v.length < 3) return; timer = setTimeout(async () => { try { const response = await fetch(`/Vendas/Clientes?telefone=${encodeURIComponent(v)}`); if (!response.ok) throw Error(); const clients = await response.json(); if (seq !== request) return; $('matches').replaceChildren(); if (!clients.length) $('matches').append(el('div', 'no-match', 'Nenhum cliente encontrado.')); for (const c of clients) { const b = el('button', '', `${c.nome} · ${c.telefone}`); b.type = 'button'; b.onclick = () => { if (busy) return; selectCustomer(c); $('phone').value = c.telefone; }; $('matches').append(b); } } catch { if (seq === request) error('Não foi possível buscar clientes.'); } }, 250); });
    $('clear').onclick = () => { if (busy || !cart.size) return; if (confirm('Limpar os itens da comanda?')) { cart.clear(); key = crypto.randomUUID(); error(''); render(); } };
    $('finish').onclick = async () => {
        if (busy || !cart.size) return; error('');
        if (mode === 'Delivery' && (!customer || !$('address').value)) { error('Selecione cliente e endereço para entrega.'); return; }
        if (mode === 'Mesa' && (!Number.isInteger(Number($('table').value)) || Number($('table').value) < 1 || Number($('table').value) > 999)) { error('Informe uma mesa entre 1 e 999.'); return; }
        busy = true; render();
        try { const response = await fetch('/Vendas/Finalizar', { method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('#csrf input').value }, body: JSON.stringify({ chaveOperacao: key, tipoAtendimento: mode, mesaNumero: mode === 'Mesa' ? Number($('table').value) : null, clienteId: customer?.id ?? null, enderecoId: mode === 'Delivery' ? Number($('address').value) : null, formatoPagamento: payment, itens: [...cart].map(([produtoId, quantidade]) => ({ produtoId, quantidade })) }) }); const result = await response.json(); if (!response.ok) throw Error(result.erro || 'Não foi possível finalizar. Confira sua sessão e tente novamente.'); window.location.assign(result.url); } catch (e) { error(e.message || 'Falha de conexão. Tente novamente para verificar o registro.'); busy = false; render(); }
    }; render();
})();
