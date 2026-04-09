/* =============================================
   InventarioQR — App JS Global
   ============================================= */

const SIDEBAR_KEY = 'sidebar_collapsed';

// ── Sidebar toggle ──
function toggleSidebar() {
    const sb = document.getElementById('sidebar');
    const mc = document.getElementById('mainContent');
    const hd = document.getElementById('header');

    const isCollapsed = sb.classList.toggle('collapsed');
    mc?.classList.toggle('collapsed', isCollapsed);
    hd?.classList.toggle('collapsed', isCollapsed);

    localStorage.setItem(SIDEBAR_KEY, isCollapsed ? '1' : '0');
}

// Restaurar estado sidebar
(function () {
    const saved = localStorage.getItem(SIDEBAR_KEY);
    if (saved === '1') {
        document.getElementById('sidebar')?.classList.add('collapsed');
        document.getElementById('mainContent')?.classList.add('collapsed');
        document.getElementById('header')?.classList.add('collapsed');
    }
})();

// ── Búsqueda global (placeholder) ──
function openSearch() {
    // Aquí puedes abrir un modal de búsqueda global AJAX
    console.log('Global search — coming soon');
}

// ── Notificaciones en tiempo real ──
(function initNotifications() {
    const notifWrap = document.getElementById('notifWrap');
    const notifBtn = document.getElementById('notifBtn');
    const notifDot = document.getElementById('notifDot');
    const notifList = document.getElementById('notifList');

    if (!notifWrap || !notifBtn || !notifDot || !notifList) return;

    function severityColor(sev) {
        if (sev === 'critico') return 'var(--color-danger)';
        if (sev === 'bajo') return 'var(--color-warning)';
        return 'var(--color-info)';
    }

    function renderItems(items) {
        if (!items || items.length === 0) {
            notifList.innerHTML = `
                <div style="padding:12px;color:var(--color-text-muted);font-size:.8125rem;">
                    Sin alertas por ahora.
                </div>`;
            return;
        }

        notifList.innerHTML = items.map(x => {
            const color = severityColor(x.severidad);
            return `
                <a href="${x.url}" style="display:block;padding:10px 12px;border-bottom:1px solid var(--color-border-light);text-decoration:none;">
                    <div style="display:flex;align-items:center;gap:8px;">
                        <span style="width:8px;height:8px;border-radius:50%;background:${color};display:inline-block;"></span>
                        <span style="font-size:.8125rem;font-weight:600;color:var(--color-text-primary);">${x.titulo}</span>
                    </div>
                    <div style="margin-left:16px;margin-top:2px;font-size:.75rem;color:var(--color-text-secondary);">
                        ${x.detalle}
                    </div>
                </a>`;
        }).join('');
    }

    async function refreshNotifications() {
        try {
            const res = await fetch('/Notificaciones/Resumen', { cache: 'no-store' });
            if (!res.ok) return;
            const data = await res.json();

            const total = data?.total ?? 0;
            notifDot.style.display = total > 0 ? 'block' : 'none';
            notifDot.title = total > 0 ? `${total} notificaciones` : '';

            renderItems(data?.items ?? []);
        } catch {
            // silencio para no afectar la UI
        }
    }

    notifBtn.addEventListener('click', () => {
        setTimeout(refreshNotifications, 80);
    });

    document.addEventListener('click', (e) => {
        if (!notifWrap.contains(e.target)) {
            notifWrap.removeAttribute('open');
        }
    });

    refreshNotifications();
    setInterval(refreshNotifications, 30000);
})();

// ── Auto-hide alerts ──
document.querySelectorAll('.alert-app').forEach(el => {
    setTimeout(() => {
        el.style.transition = 'opacity .4s';
        el.style.opacity = '0';
        setTimeout(() => el.remove(), 400);
    }, 4000);
});

// ── Confirm delete ──
function confirmDelete(url, msg) {
    if (confirm(msg || '¿Estás seguro de que deseas eliminar este registro?')) {
        window.location.href = url;
    }
}

// ── Formato de números ──
function formatNumber(n) {
    return new Intl.NumberFormat('es-GT').format(n);
}