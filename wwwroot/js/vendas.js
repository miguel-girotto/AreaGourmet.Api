'use strict';
(() => {
  const $ = s => document.querySelector(s);
  const money = n => n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  const cart = new Map();
  let mode = 'Retirada', cliente = null, category = 'all', submitting = false, debounce, searchVersion = 0;
  // A chave continua a mesma após erro de rede; reenviar não cria outra venda.
  const key = $('#sale-token').dataset.key;
  function render() {
    const target = $('#cart-items'); target.replaceChildren();
    if (!cart.size) {
      const box = document.createElement('div'); box.className = 'cart-empty';
      const h = document.createElement('h3'); h.textContent = 'Vamos montar um pedido?';
      const p = document.createElement('p'); p.textContent = 'Adicione produtos do cardápio para começar.';
      box.append(h, p); target.append(box);
    }
    let subtotal = 0;
    cart.forEach(item => {
      subtotal += item.price * item.quantity;
      const row = document.createElement('div'); row.className = 'cart-item';
      const info = document.createElement('div'); info.className = 'cart-item-info';
      const title = document.createElement('h3'); title.textContent = item.name;
      const price = document.createElement('small'); price.textContent = money(item.price) + ' / unidade'; info.append(title, price);
      const quantity = document.createElement('div'); quantity.className = 'quantity';
      for (const delta of [-1, 0, 1]) {
        if (!delta) { const number = document.createElement('span'); number.textContent = item.quantity; quantity.append(number); continue; }
        const b = document.createElement('button'); b.type = 'button'; b.textContent = delta < 0 ? '−' : '+';
        b.setAttribute('aria-label', (delta < 0 ? 'Diminuir ' : 'Aumentar ') + item.name);
        b.addEventListener('click', () => { if (submitting) return; item.quantity += delta; if (item.quantity <= 0) cart.delete(item.id); if (item.quantity > 999) item.quantity = 999; render(); }); quantity.append(b);
      }
      const total = document.createElement('span'); total.className = 'item-total'; total.textContent = money(item.price * item.quantity);
      const remove = document.createElement('button'); remove.type = 'button'; remove.className = 'remove-item'; remove.setAttribute('aria-label', 'Remover ' + item.name);
      remove.innerHTML = '<svg><use href="/img/icons.svg#trash"></use></svg>';
      remove.addEventListener('click', () => { if (!submitting) { cart.delete(item.id); render(); } });
      row.append(info, quantity, total, remove); target.append(row);
    });
    const fee = mode === 'Delivery' ? Number($('#taxa').value || 0) : 0;
    $('#subtotal').textContent = money(subtotal); $('#fee-total').textContent = money(fee); $('#total').textContent = money(subtotal + fee);
    $('#checkout-total').textContent = cart.size ? '(' + money(subtotal + fee) + ')' : '';
    $('#cart-mode').textContent = mode + ' · novo pedido';
  }
  document.querySelectorAll('[data-mode]').forEach(b => b.addEventListener('click', () => {
    if (submitting) return;
    mode = b.dataset.mode;
    document.querySelectorAll('[data-mode]').forEach(x => x.classList.toggle('selected', x === b));
    $('#table-field').hidden = mode !== 'Mesa'; $('#fee-field').hidden = mode !== 'Delivery'; $('#address-field').hidden = mode !== 'Delivery'; render();
  }));
  document.querySelectorAll('.add-product').forEach(b => b.addEventListener('click', () => {
    if (submitting) return;
    const id = Number(b.dataset.id);
    if (!cart.has(id)) cart.set(id, { id, name: b.dataset.name, price: Number(b.dataset.price), quantity: 0 });
    cart.get(id).quantity = Math.min(999, cart.get(id).quantity + 1); render();
  }));
  function filter() {
    const search = $('#product-search').value.trim().toLocaleLowerCase('pt-BR'); let count = 0;
    document.querySelectorAll('.product-card').forEach(p => { p.hidden = !((category === 'all' || p.dataset.category === category) && p.dataset.name.includes(search)); if (!p.hidden) count++; });
    $('#no-products').hidden = count > 0;
  }
  document.querySelectorAll('[data-category]:not(.product-card)').forEach(b => b.addEventListener('click', () => {
    category = b.dataset.category; document.querySelectorAll('.category-tabs button').forEach(x => x.classList.toggle('selected', x === b)); filter();
  }));
  $('#product-search').addEventListener('input', filter); $('#taxa').addEventListener('input', render);
  $('#clear-cart').addEventListener('click', () => { if (!submitting && (!cart.size || confirm('Limpar os itens desta comanda?'))) { cart.clear(); render(); } });
  function selectCliente(c) {
    cliente = c; $('#customer-name').textContent = c ? c.nome : 'Venda sem identificação'; $('#remove-customer').hidden = !c;
    $('#customer-results').replaceChildren(); $('#endereco').replaceChildren(new Option('Selecione o endereço', ''));
    if (c) c.enderecos.forEach(e => $('#endereco').append(new Option(e.texto, e.id)));
  }
  $('#remove-customer').addEventListener('click', () => selectCliente(null));
  $('#telefone').addEventListener('input', () => {
    clearTimeout(debounce); const version = ++searchVersion;
    const value = $('#telefone').value.replace(/\D/g, ''); $('#customer-results').replaceChildren();
    if (value.length < 3) return;
    debounce = setTimeout(async () => {
      try {
        const response = await fetch('/api/clientes?telefone=' + encodeURIComponent(value));
        if (!response.ok) throw Error('Não foi possível buscar clientes. Verifique sua sessão.');
        const data = await response.json(); if (version !== searchVersion) return;
        $('#customer-results').replaceChildren();
        if (!data.length) { const p = document.createElement('p'); p.textContent = 'Nenhum cliente encontrado.'; $('#customer-results').append(p); }
        data.forEach(c => { const b = document.createElement('button'); b.type = 'button'; b.textContent = c.nome + ' · ' + c.telefone; b.addEventListener('click', () => selectCliente(c)); $('#customer-results').append(b); });
      } catch (e) { if (version === searchVersion) { const p = document.createElement('p'); p.textContent = e.message; $('#customer-results').append(p); } }
    }, 250);
  });
  async function send(finalizar) {
    if (submitting) return;
    const error = $('#sale-error'); error.hidden = true;
    const fail = message => { error.textContent = message; error.hidden = false; };
    if (!cart.size) return fail('Adicione pelo menos um produto.');
    if (mode === 'Mesa' && (!Number.isInteger(Number($('#mesa').value)) || Number($('#mesa').value) < 1 || Number($('#mesa').value) > 999)) return fail('Informe uma mesa de 1 a 999.');
    if (mode === 'Delivery' && (!cliente || !$('#endereco').value)) return fail('Selecione o cliente e um endereço para Delivery.');
    if (mode === 'Delivery' && !$('#taxa').checkValidity()) return fail('Informe uma taxa de entrega válida.');
    submitting = true; $('#checkout').disabled = true; $('#save-order').disabled = true;
    try {
      const response = await fetch('/api/pedidos', { method: 'POST', headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': $('#sale-token input').value }, body: JSON.stringify({
        chaveOperacao: key, tipoAtendimento: mode, numeroMesa: mode === 'Mesa' ? Number($('#mesa').value) : null,
        clienteId: cliente?.id ?? null, enderecoId: mode === 'Delivery' ? Number($('#endereco').value) : null,
        taxaEntrega: mode === 'Delivery' ? Number($('#taxa').value) : 0,
        formaPagamento: $('input[name="pagamento"]:checked').value, finalizar,
        itens: [...cart.values()].map(i => ({ produtoId: i.id, quantidade: i.quantity }))
      }) });
      if (response.status === 401) throw Error('Sua sessão expirou. Entre novamente antes de continuar.');
      const data = await response.json().catch(() => ({}));
      if (!response.ok) throw Error(data.erro || 'Não foi possível registrar. Confira o histórico de pedidos antes de reenviar.');
      location.assign(data.url);
    } catch (e) { fail(e.message || 'Falha de conexão. Confira o histórico antes de reenviar.'); submitting = false; $('#checkout').disabled = false; $('#save-order').disabled = false; }
  }
  $('#checkout').addEventListener('click', () => send(true)); $('#save-order').addEventListener('click', () => send(false));
  window.addEventListener('beforeunload', e => { if (cart.size && !submitting) { e.preventDefault(); e.returnValue = ''; } });
})();
