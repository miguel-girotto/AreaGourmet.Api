document.querySelectorAll('form.confirm').forEach(form=>form.addEventListener('submit',e=>{if(!confirm('Inativar este registro? O histórico será preservado.'))e.preventDefault();}));
document.getElementById('print')?.addEventListener('click',()=>window.print());
