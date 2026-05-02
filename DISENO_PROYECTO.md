# Documentación de Diseño - InventarioQR

Este documento contiene toda la especificación de diseño del proyecto para aplicar a otros proyectos.

---

## 1. Paleta de Colores

### Colores Base
| Variable CSS | Valor | Uso |
|-------------|-------|-----|
| `--color-bg` | `#f8f9fa` | Fondo principal de la aplicación |
| `--color-white` | `#ffffff` | Fondo de tarjetas, sidebar, header |
| `--color-border` | `#e5e7eb` | Bordes de tarjetas, tablas, inputs |
| `--color-border-light` | `#f3f4f6` | Fondos de tablas, hover |

### Colores de Texto
| Variable CSS | Valor | Uso |
|-------------|-------|-----|
| `--color-text-primary` | `#111827` | Texto principal, títulos |
| `--color-text-secondary` | `#6b7280` | Texto secundario, etiquetas |
| `--color-text-muted` | `#9ca3af` | Placeholders, texto deshabilitado |

### Colores de Acento
| Variable CSS | Valor | Uso |
|-------------|-------|-----|
| `--color-accent` | `#2563eb` | Links, iconos activos, elementos destacados |
| `--color-accent-hover` | `#1d4ed8` | Hover de elementos accent |
| `--color-accent-light` | `#eff6ff` | Fondo de elementos activos en sidebar |

### Colores de Estado
| Variable CSS | Valor | Uso |
|-------------|-------|-----|
| `--color-success` | `#16a34a` | Estados exitosos, completado |
| `--color-success-light` | `#f0fdf4` | Fondo success |
| `--color-warning` | `#d97706` | Alertas, advertencias |
| `--color-warning-light` | `#fffbeb` | Fondo warning |
| `--color-danger` | `#dc2626` | Errores, eliminar, crítico |
| `--color-danger-light` | `#fef2f2` | Fondo danger |
| `--color-info` | `#0891b2` | Información, en tránsito |
| `--color-info-light` | `#ecfeff` | Fondo info |

---

## 2. Tipografía

### Familia de Fuente
```css
font-family: 'Inter', sans-serif;
```

**Google Fonts Import:**
```
https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap
```

### Tamaños de Fuente
| Elemento | Tamaño | Peso |
|----------|--------|------|
| Body base | `0.9375rem` (15px) | 400 |
| Títulos página | `1.25rem` (20px) | 700 |
| Títulos card | `0.9375rem` (15px) | 600 |
| Nav items | `0.875rem` (14px) | 450 |
| Tablas headers | `0.75rem` (12px) | 600 |
| Tablas celdas | `0.875rem` (14px) | 400 |
| Badges | `0.75rem` (12px) | 500-600 |
| Labels | `0.8125rem` (13px) | 500 |
| KPI valores | `1.75rem` (28px) | 700 |
| KPI etiquetas | `0.75rem` (12px) | 700 |

### Pesos Disponibles
- 300 - Light
- 400 - Regular
- 450 - Medium
- 500 - Semi Bold
- 600 - Bold
- 700 - Extra Bold

---

## 3. Espaciado y Dimensiones

### Dimensiones del Layout
| Elemento | Valor |
|----------|-------|
| Ancho sidebar | `240px` |
| Sidebar colapsado | `64px` |
| Altura header | `56px` |
| Padding contenido principal | `28px 32px` |

### Spacing (Gap)
| Nombre | Valor |
|--------|-------|
| xs | 4px |
| sm | 8px |
| md | 16px |
| lg | 24px |
| xl | 32px |
| 2xl | 48px |

### Radios (Border Radius)
| Variable | Valor |
|----------|-------|
| `--radius` | `2px` (principal) |
| Badges | `20px` (circular) |
| Botones | `2px` |
| Inputs | `2px` |

---

## 4. Sombras

| Variable | Valor |
|----------|-------|
| `--shadow-sm` | `0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04)` |
| `--shadow-md` | `0 4px 12px rgba(0,0,0,0.08)` |
| `--shadow-lg` | `0 8px 32px rgba(0,0,0,0.10)` |

---

## 5. Transiciones

```css
--transition: all 0.18s ease;
```

---

## 6. Componentes

### 6.1 Sidebar

```css
.sidebar {
    position: fixed;
    top: 0;
    left: 0;
    width: var(--sidebar-width);  /* 240px */
    height: 100vh;
    background: var(--color-white);
    border-right: 1px solid var(--color-border);
    z-index: 100;
}

/* Logo */
.logo-icon {
    width: 32px;
    height: 32px;
    background: var(--color-text-primary);
    border-radius: var(--radius);
    font-size: 1rem;
}

.logo-text {
    font-size: 0.9375rem;
    font-weight: 700;
}

/* Nav Items */
.nav-item {
    padding: 9px 16px;
    font-size: 0.875rem;
    font-weight: 450;
    margin: 1px 8px;
    border-radius: var(--radius);
    gap: 10px;
}

.nav-item.active {
    background: var(--color-accent-light);
    color: var(--color-accent);
    font-weight: 500;
}

/* Nav Groups */
.nav-group-label {
    font-size: 0.6875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: .08em;
    color: var(--color-text-muted);
    padding: 8px 16px 4px;
}

/* User Avatar */
.user-avatar {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background: var(--color-accent);
    color: #fff;
    font-size: 0.75rem;
    font-weight: 600;
}
```

### 6.2 Header

```css
.header {
    position: fixed;
    top: 0;
    left: var(--sidebar-width);
    right: 0;
    height: var(--header-height);  /* 56px */
    background: var(--color-white);
    border-bottom: 1px solid var(--color-border);
    padding: 0 24px;
    z-index: 99;
}

.header-toggle,
.header-btn {
    width: 36px;
    height: 36px;
    border-radius: var(--radius);
    color: var(--color-text-secondary);
}

.header-breadcrumb {
    font-size: 0.875rem;
}
```

### 6.3 Tarjetas (Cards)

```css
.card {
    background: var(--color-white);
    border: 1px solid var(--color-border);
    border-radius: var(--radius);  /* 2px */
    box-shadow: var(--shadow-sm);
}

.card-header {
    padding: 16px 20px;
    border-bottom: 1px solid var(--color-border);
}

.card-title {
    font-size: 0.9375rem;
    font-weight: 600;
}

.card-body {
    padding: 20px;
}
```

### 6.4 Stat Cards (KPIs)

```css
.stat-card {
    background: var(--color-white);
    border: 1px solid var(--color-border);
    border-radius: var(--radius);
    padding: 14px 16px;
    min-height: 76px;
}

.stat-icon {
    width: 26px;
    height: 26px;
    font-size: 1rem;
}

.stat-value {
    font-size: 1.75rem;
    font-weight: 700;
    color: #111;
}

.stat-label {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: .08em;
    color: #64748b;
}
```

### 6.5 Tablas

```css
.table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.875rem;
}

.table th {
    text-align: left;
    padding: 10px 16px;
    font-size: 0.75rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: .05em;
    color: var(--color-text-muted);
    background: var(--color-border-light);
    border-bottom: 1px solid var(--color-border);
    white-space: nowrap;
}

.table td {
    padding: 12px 16px;
    border-bottom: 1px solid var(--color-border-light);
    color: var(--color-text-primary);
    vertical-align: middle;
}

.table tr:hover td {
    background: #fafafa;
}
```

### 6.6 Botones

**Botón Primario:**
```css
.btn-primary-app,
.btn-success-app {
    background: #0e7490;       /* Cyan oscuro */
    color: #fff;
    border: none;
    border-radius: var(--radius);
    padding: 8px 16px;
    font-size: 0.875rem;
    font-weight: 500;
    display: inline-flex;
    align-items: center;
    gap: 6px;
}

.btn-primary-app:hover {
    background: #0c657d;
}
```

**Botón Secundario:**
```css
.btn-secondary-app {
    background: var(--color-white);
    color: var(--color-text-primary);
    border: 1px solid var(--color-border);
    border-radius: var(--radius);
    padding: 8px 16px;
    font-size: 0.875rem;
    font-weight: 500;
}

.btn-secondary-app:hover {
    background: var(--color-border-light);
}
```

**Botón Peligro:**
```css
.btn-danger-app {
    background: var(--color-danger-light);
    color: var(--color-danger);
    border: none;
    border-radius: var(--radius);
    padding: 8px 16px;
    font-size: 0.875rem;
    font-weight: 500;
}
```

**Botón Icono:**
```css
.btn-icon {
    background: none;
    border: 1px solid var(--color-border);
    border-radius: var(--radius);
    width: 32px;
    height: 32px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
}
```

### 6.7 Badges / Etiquetas

**Badge con punto (Status):**
```css
.badge-status {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    font-size: 0.75rem;
    font-weight: 500;
    padding: 3px 10px;
    border-radius: 20px;
}

.badge-status::before {
    content: '';
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: currentColor;
}

/* Variantes */
.badge-activo {
    background: var(--color-success-light);
    color: var(--color-success);
}

.badge-inactivo {
    background: var(--color-border-light);
    color: var(--color-text-muted);
}

.badge-alerta {
    background: var(--color-warning-light);
    color: var(--color-warning);
}

.badge-critico {
    background: var(--color-danger-light);
    color: var(--color-danger);
}

.badge-transito {
    background: var(--color-info-light);
    color: var(--color-info);
}
```

**Badges Sólidos (sin punto):**
```css
.badge-completada-solid {
    background: var(--color-info);
    color: #fff;
    border-radius: 4px;
    font-weight: 600;
    padding: 4px 10px;
}

.badge-activa-solid {
    background: var(--color-success);
    color: #fff;
    border-radius: 4px;
    font-weight: 600;
    padding: 4px 10px;
}

.badge-vencida-solid {
    background: var(--color-danger);
    color: #fff;
    border-radius: 4px;
    font-weight: 600;
    padding: 4px 10px;
}

.badge-cancelada-solid {
    background: #6b7280;
    color: #fff;
    border-radius: 4px;
    font-weight: 600;
    padding: 4px 10px;
}
```

### 6.8 Formularios / Inputs

```css
.form-label {
    font-size: 0.8125rem;
    font-weight: 500;
    color: #374151;
    margin-bottom: 6px;
}

.form-control {
    border: 1px solid #e5e7eb;
    border-radius: 2px;
    padding: 10px 14px;
    font-size: 0.9375rem;
    color: #111;
    transition: border-color .15s, box-shadow .15s;
}

.form-control:focus {
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59,130,246,0.12);
    outline: none;
}
```

### 6.9 Alertas

```css
.alert-app {
    display: flex;
    align-items: flex-start;
    gap: 12px;
    padding: 12px 16px;
    border-radius: var(--radius);
    font-size: 0.875rem;
    margin-bottom: 16px;
}

.alert-app.warning {
    background: var(--color-warning-light);
    border-left: 3px solid var(--color-warning);
    color: #92400e;
}

.alert-app.danger {
    background: var(--color-danger-light);
    border-left: 3px solid var(--color-danger);
    color: #991b1b;
}

/* Alerta success inline */
.alert-app.success-inline {
    background: #f0fdf4;
    border-left: 3px solid #16a34a;
    color: #166534;
}
```

### 6.10 Modal de Confirmación

```css
.confirm-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0,0,0,0.35);
    z-index: 999;
    display: flex;
    align-items: center;
    justify-content: center;
    animation: fadeInOverlay .15s ease;
}

.confirm-box {
    background: #fff;
    border-radius: 2px;
    padding: 36px 32px 28px;
    width: 100%;
    max-width: 400px;
    text-align: center;
    box-shadow: 0 20px 60px rgba(0,0,0,0.18);
    animation: slideUp .18s ease;
}

.confirm-icon {
    width: 60px;
    height: 60px;
    border-radius: 2px;
    border: 3px solid #d1d5db;
    color: #9ca3af;
    font-size: 1.75rem;
    font-weight: 700;
}

.confirm-title {
    font-size: 1.125rem;
    font-weight: 700;
    color: var(--color-text-primary);
    margin-bottom: 8px;
}

.confirm-msg {
    font-size: 0.875rem;
    color: var(--color-text-secondary);
    margin-bottom: 24px;
    line-height: 1.5;
}

.confirm-actions {
    display: flex;
    justify-content: center;
    gap: 12px;
}
```

### 6.11 Toast Notification (Delete Success)

```css
.delete-success-overlay {
    position: fixed;
    top: 18px;
    left: 50%;
    transform: translateX(-50%);
    z-index: 1200;
    display: flex;
    animation: deleteToastFadeOut 2.4s ease forwards;
}

.delete-success-box {
    display: inline-flex;
    align-items: center;
    gap: 10px;
    background: #16a34a;
    color: #fff;
    border-radius: 6px;
    padding: 10px 14px;
    box-shadow: var(--shadow-md);
    font-size: 0.875rem;
    font-weight: 500;
}

@keyframes deleteToastFadeOut {
    0% { opacity: 0; transform: translateX(-50%) translateY(-6px); }
    12% { opacity: 1; transform: translateX(-50%) translateY(0); }
    82% { opacity: 1; transform: translateX(-50%) translateY(0); }
    100% { opacity: 0; transform: translateX(-50%) translateY(-6px); }
}
```

### 6.12 Page Header

```css
.page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 24px;
}

.page-title {
    font-size: 1.25rem;
    font-weight: 700;
    color: var(--color-text-primary);
}

.page-subtitle {
    font-size: 0.875rem;
    color: var(--color-text-secondary);
    margin-top: 2px;
}
```

### 6.13 Empty State

```css
.empty-state {
    text-align: center;
    padding: 56px 24px;
    color: var(--color-text-muted);
}

.empty-state i {
    font-size: 2.5rem;
    margin-bottom: 12px;
    display: block;
}

.empty-state p {
    font-size: 0.9375rem;
}
```

### 6.14 Login Page (Special)

```css
.login-wrapper {
    width: 100%;
    max-width: 460px;
    border-radius: 2px;
    overflow: hidden;
    box-shadow: 0 4px 32px rgba(0,0,0,0.10);
    background: #fff;
}

.login-form-panel {
    width: 100%;
    background: #fff;
    padding: 56px 48px;
}

.login-form-panel h2 {
    font-size: 1.375rem;
    font-weight: 700;
    color: #111;
    margin-bottom: 4px;
}

.login-form-panel .subtitle {
    font-size: 0.875rem;
    color: #6b7280;
    margin-bottom: 36px;
}

.btn-login {
    background: #0e7490;
    color: #fff;
    border: none;
    border-radius: 2px;
    padding: 11px;
    font-size: 0.9375rem;
    font-weight: 500;
    width: 100%;
    cursor: pointer;
    transition: background .15s;
    margin-top: 8px;
}
```

---

## 7. Grillas Responsivas

```css
.grid-4 {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;
}

.grid-3 {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 16px;
}

.grid-2 {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
}

/* Breakpoints */
@media (max-width: 1200px) {
    .grid-4 { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 900px) {
    .grid-3, .grid-2 { grid-template-columns: 1fr; }
}

@media (max-width: 768px) {
    .main-content {
        margin-left: 0 !important;
        padding: 16px;
    }
    .sidebar { transform: translateX(-100%); }
    .sidebar.mobile-open { transform: translateX(0); }
    .header { left: 0 !important; }
    .grid-4 { grid-template-columns: 1fr; }
}
```

---

## 8. Scrollbar Personalizada

```css
::-webkit-scrollbar {
    width: 5px;
    height: 5px;
}

::-webkit-scrollbar-track {
    background: transparent;
}

::-webkit-scrollbar-thumb {
    background: var(--color-border);
    border-radius: 10px;
}

::-webkit-scrollbar-thumb:hover {
    background: #c0c0c0;
}
```

---

## 9. Iconos

Se utiliza **Bootstrap Icons**:
```
https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css
```

### Iconos Comunes Utilizados
- `bi-grid-1x2` - Dashboard
- `bi-arrow-left-right` - Movimientos
- `bi-clipboard-check` - Operaciones
- `bi-bookmark` - Reservas
- `bi-box-seam` - Inventario
- `bi-tag` - Productos
- `bi-qr-code` - Códigos QR
- `bi-upc-scan` - Escaneo
- `bi-building` - Bodega
- `bi-people` - Usuarios
- `bi-bar-chart` - Reportes
- `bi-shield-check` - Auditoría
- `bi-check-circle-fill` - Éxito
- `bi-exclamation-circle-fill` - Error
- `bi-eye` / `bi-eye-slash` - Mostrar/Ocultar password

---

## 10. Bibliotecas Externas Requeridas

### CSS
1. **Google Fonts - Inter**
   ```
   https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap
   ```

2. **Bootstrap 5.3.2**
   ```
   https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css
   ```

3. **Bootstrap Icons 1.11.3**
   ```
   https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css
   ```

### JavaScript
1. **Bootstrap 5.3.2 Bundle**
   ```
   https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js
   ```

---

## 11. Resumen - Quick Copy

### CSS Variables Completo
```css
:root {
    --sidebar-width: 240px;
    --sidebar-collapsed: 64px;
    --header-height: 56px;
    --color-bg: #f8f9fa;
    --color-white: #ffffff;
    --color-border: #e5e7eb;
    --color-border-light: #f3f4f6;
    --color-text-primary: #111827;
    --color-text-secondary: #6b7280;
    --color-text-muted: #9ca3af;
    --color-accent: #2563eb;
    --color-accent-hover: #1d4ed8;
    --color-accent-light: #eff6ff;
    --color-success: #16a34a;
    --color-success-light: #f0fdf4;
    --color-warning: #d97706;
    --color-warning-light: #fffbeb;
    --color-danger: #dc2626;
    --color-danger-light: #fef2f2;
    --color-info: #0891b2;
    --color-info-light: #ecfeff;
    --shadow-sm: 0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);
    --shadow-md: 0 4px 12px rgba(0,0,0,0.08);
    --shadow-lg: 0 8px 32px rgba(0,0,0,0.10);
    --radius: 2px;
    --transition: all 0.18s ease;
}
```

### Button Primary
```css
.btn-primary-app {
    background: #0e7490;
    color: #fff;
    border: none;
    border-radius: 2px;
    padding: 8px 16px;
    font-size: 0.875rem;
    font-weight: 500;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    gap: 6px;
    transition: all 0.18s ease;
}
```

---

## 12. Notas de Implementación

- El diseño está inspirado en **Stripe + Notion** (minimalista, limpio)
- El color de acento principal es `#0e7490` (cyan oscuro) para botones
- El color de acento del sidebar es `#2563eb` (azul)
- Todos los elementos usan `border-radius: 2px` (muy sutil)
- La fuente Inter proporciona buena legibilidad
- Los badges tienen estilo diferente según el contexto (con punto vs sólido)
- El diseño es responsive con mobile-first en mente
