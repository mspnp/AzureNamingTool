# Design Implementation Plan - DesignLinear Modernization

## Overview
This document outlines the plan to implement the new DesignLinear design across the entire Azure Naming Tool application. The new design features a dark gray sidebar with white text, Azure-inspired color accents (blue, teal, cyan, purple), and modern minimal styling.

## Current State
- **Current Design**: Bootstrap-based with traditional components
- **New Design**: DesignLinear mockup (dark gray sidebar, Azure colors, modern minimal)
- **Mockup Location**: `src/Components/Pages/DesignMockups/DesignLinear.razor`
- **Status**: Design approved, ready for implementation

## Design System - Color Palette

### Sidebar Colors
- **Background**: `linear-gradient(180deg, #2d3748 0%, #1a202c 100%)`
- **Text**: `rgba(255, 255, 255, 0.85)` (nav items), `#ffffff` (active/hover)
- **Borders**: `rgba(255, 255, 255, 0.1)`
- **Hover**: `rgba(255, 255, 255, 0.1)` background
- **Active**: `rgba(255, 255, 255, 0.15)` background

### Main Content Colors
- **Background**: `#fafafa` (body), `#ffffff` (cards/panels)
- **Borders**: `#e5e5e5`
- **Text Primary**: `#171717`
- **Text Secondary**: `#737373`

### Accent Colors (Azure-inspired)
- **Primary Blue** (buttons, main actions): `#0078d4` → `#005a9e`
- **Teal** (stats, data values): `#00b7c3`
- **Bright Cyan** (section titles): `#50e6ff`
- **Purple** (activity icons, variety): `#8661c5`
- **Green** (success indicators): `#059669` (keep existing)

### Component Styling
- **Border Radius**: `6px` (inputs), `8px` (cards)
- **Shadows**: Subtle on hover, `0 2px 8px` for buttons
- **Transitions**: `0.15s ease` for all interactions
- **Typography**: `-apple-system, BlinkMacSystemFont, "Segoe UI", "Inter", "Roboto", "Helvetica Neue", Arial, sans-serif`

## Light/Dark Mode Theme System

### Overview
The application will support both Light and Dark modes with a user-controlled toggle. The theme preference will be persisted in browser localStorage and respected across sessions. The design system already includes a dark sidebar, so dark mode will primarily affect the main content area.

### Theme Colors

#### Light Mode (Default)
- **Background**: `#fafafa` (body), `#ffffff` (cards/panels)
- **Borders**: `#e5e5e5`
- **Text Primary**: `#171717`
- **Text Secondary**: `#737373`
- **Sidebar**: Dark gray gradient (remains same)
- **Stat Card Hover**: `#f0fafb` (light teal tint)
- **Activity Item Hover**: `#fafafa`

#### Dark Mode
- **Background**: `#0d1117` (body), `#161b22` (cards/panels)
- **Borders**: `#30363d`
- **Text Primary**: `#e6edf3`
- **Text Secondary**: `#8b949e`
- **Sidebar**: Dark gray gradient (remains same)
- **Stat Card Hover**: `#1c2128` (darker on hover)
- **Activity Item Hover**: `#1c2128`

### Implementation Strategy

#### 1. CSS Variables Approach
Use CSS custom properties (variables) for all theme-dependent colors. Toggle theme by changing `data-theme` attribute on `<html>` or `<body>` element.

**CSS Structure**:
```css
/* Default Light Theme */
:root {
    --color-bg-primary: #fafafa;
    --color-bg-secondary: #ffffff;
    --color-border: #e5e5e5;
    --color-text-primary: #171717;
    --color-text-secondary: #737373;
    --color-hover-bg: #fafafa;
    --color-card-hover-bg: #f0fafb;
    
    /* Accent colors remain consistent */
    --color-azure-blue: #0078d4;
    --color-azure-teal: #00b7c3;
    --color-cyan-bright: #50e6ff;
    --color-purple: #8661c5;
    --color-green: #059669;
}

/* Dark Theme */
[data-theme="dark"] {
    --color-bg-primary: #0d1117;
    --color-bg-secondary: #161b22;
    --color-border: #30363d;
    --color-text-primary: #e6edf3;
    --color-text-secondary: #8b949e;
    --color-hover-bg: #1c2128;
    --color-card-hover-bg: #1c2128;
}
```

#### 2. Theme Toggle Component
**File**: `src/Components/General/ThemeToggle.razor`

**Features**:
- Toggle button in header (sun/moon icons)
- Smooth transition between themes
- Persist preference in localStorage
- Initialize theme on app load

**Component Structure**:
```razor
<button class="theme-toggle-btn" @onclick="ToggleTheme">
    @if (currentTheme == "light")
    {
        <span class="theme-icon">🌙</span>
        <span class="theme-label">Dark Mode</span>
    }
    else
    {
        <span class="theme-icon">☀️</span>
        <span class="theme-label">Light Mode</span>
    }
</button>

@code {
    private string currentTheme = "light";
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            currentTheme = await JSRuntime.InvokeAsync<string>("getTheme");
            StateHasChanged();
        }
    }
    
    private async Task ToggleTheme()
    {
        currentTheme = currentTheme == "light" ? "dark" : "light";
        await JSRuntime.InvokeVoidAsync("setTheme", currentTheme);
    }
}
```

#### 3. JavaScript Theme Helper
**File**: `src/wwwroot/js/theme.js`

```javascript
// Initialize theme on page load
function initTheme() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);
    return savedTheme;
}

// Get current theme
function getTheme() {
    return localStorage.getItem('theme') || 'light';
}

// Set theme
function setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
    
    // Add transition class temporarily for smooth transition
    document.documentElement.classList.add('theme-transitioning');
    setTimeout(() => {
        document.documentElement.classList.remove('theme-transitioning');
    }, 300);
}

// Initialize on load
initTheme();
```

#### 4. Theme Transition CSS
**File**: `src/wwwroot/css/modern-design.css`

```css
/* Smooth theme transitions */
html.theme-transitioning,
html.theme-transitioning *,
html.theme-transitioning *:before,
html.theme-transitioning *:after {
    transition: background-color 0.3s ease, 
                color 0.3s ease, 
                border-color 0.3s ease !important;
    transition-delay: 0s !important;
}
```

### Phase Implementation

#### Phase 1.4: Theme System Foundation (New - Est. 3-4 hours)
**Goal**: Implement CSS variable system and theme infrastructure

**Tasks**:
- [ ] Create CSS variables for all theme-dependent colors
- [ ] Add `data-theme` attribute support in CSS
- [ ] Create `theme.js` file with localStorage management
- [ ] Add theme transition CSS classes
- [ ] Test theme switching works correctly
- [ ] Verify all colors properly use CSS variables

#### Phase 5.5: Theme Toggle Component (New - Est. 2-3 hours)
**Goal**: Create and integrate theme toggle UI component

**Tasks**:
- [ ] Create `ThemeToggle.razor` component
- [ ] Add toggle button styling (consistent with design system)
- [ ] Integrate component into main header
- [ ] Add keyboard accessibility (Space/Enter to toggle)
- [ ] Add screen reader labels (aria-label)
- [ ] Test theme toggle in all pages
- [ ] Test theme persistence across page navigation
- [ ] Test theme initialization on app load

### Considerations

#### Sidebar Theme Behavior
**Decision**: Keep sidebar dark in both modes
- The dark gray gradient sidebar remains constant in both themes
- This provides consistent navigation experience
- Dark sidebars are common in professional tools
- Easier to implement (less CSS to maintain)

**Alternative**: Make sidebar theme-aware
- Light mode: Light gray sidebar
- Dark mode: Dark gray sidebar (current)
- Requires additional CSS variables for sidebar
- Adds complexity but provides full theme consistency

#### Admin-Only Theme Settings
**Option**: Add admin preference for default theme
- Admin can set organization-wide default (light/dark)
- Users can still override with toggle
- Stored in configuration settings
- **Recommendation**: Implement in Phase 8 (post-launch enhancement)

#### Respect System Preference
**Option**: Auto-detect system dark/light mode
- Use `prefers-color-scheme` media query
- Check on first visit before localStorage exists
- **Implementation**:
```javascript
function getSystemTheme() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function initTheme() {
    const savedTheme = localStorage.getItem('theme');
    const theme = savedTheme || getSystemTheme();
    document.documentElement.setAttribute('data-theme', theme);
    return theme;
}
```

### Testing Requirements

#### Visual Testing
- [ ] Test all pages in light mode (current design)
- [ ] Test all pages in dark mode (new implementation)
- [ ] Test theme toggle button functionality
- [ ] Test theme transitions are smooth (no flash)
- [ ] Verify stat card values still use Azure teal in both modes
- [ ] Verify buttons use Azure blue gradient in both modes
- [ ] Verify activity icons use purple gradient in both modes

#### Functional Testing
- [ ] Test theme persists across page navigation
- [ ] Test theme persists across browser refresh
- [ ] Test theme persists across browser close/reopen
- [ ] Test theme toggle works in all major browsers (Chrome, Edge, Firefox, Safari)
- [ ] Test localStorage fallback if disabled
- [ ] Test theme initialization on first visit

#### Accessibility Testing
- [ ] Test theme toggle with keyboard (Tab, Enter, Space)
- [ ] Test theme toggle with screen reader
- [ ] Verify color contrast meets WCAG AA in both themes
  - Light mode: Dark text on light backgrounds
  - Dark mode: Light text on dark backgrounds
- [ ] Test focus indicators visible in both themes

### Updated Timeline

**New Time Estimates**:
- Phase 1.4: Theme System Foundation: **3-4 hours**
- Phase 5.5: Theme Toggle Component: **2-3 hours**
- **Additional Testing Time**: **1-2 hours**

**Total Additional Time**: **6-9 hours**

**Original Estimate**: 33-46 hours  
**New Estimate with Light/Dark Mode**: **39-55 hours**

---

## Bootstrap Removal & Modern Component System

### Overview
**CRITICAL DECISION**: Remove Bootstrap entirely from the application and replace it with a custom, modern design system. Bootstrap's dated collapsible card pattern and general styling conflicts with the DesignLinear vision.

### Why Remove Bootstrap?

#### Current Problems with Bootstrap
1. **Dated UI Patterns**: Bootstrap collapsible cards (`.card`, `.card-header`, `data-bs-toggle="collapse"`) feel old and clunky
2. **Inconsistent Design Language**: Bootstrap's design system conflicts with Azure's modern aesthetic
3. **Heavy CSS Overhead**: Shipping entire Bootstrap library when we only need custom components
4. **Limited Customization**: Hard to override Bootstrap's opinionated styles
5. **Accessibility Issues**: Bootstrap's collapse mechanism is complex and can have a11y problems
6. **Mobile Experience**: Bootstrap modals and cards don't provide the smooth mobile UX we want

#### Benefits of Custom Design System
1. **Modern UI Patterns**: Clean, minimal components with smooth animations
2. **Performance**: Only load CSS we actually use (~70% smaller stylesheet)
3. **Full Control**: Every component designed exactly as we want
4. **Consistency**: Single source of truth for design tokens (colors, spacing, typography)
5. **Maintainability**: Easier to update and extend without framework conflicts
6. **Better Mobile UX**: Custom components optimized for touch interactions

### Current Bootstrap Usage Analysis

#### Bootstrap Components Currently Used
Based on codebase analysis, we're using:

**Layout & Grid**
- `.container`, `.container-fluid` → Replace with custom `.modern-container`
- `.row`, `.col-*` → Replace with CSS Grid or Flexbox

**Cards**
- `.card`, `.card-body`, `.card-header` → Replace with `.modern-card`, `.modern-section`
- `.card.mb-3`, `.card.@theme.ThemeStyle` → Replace with modern section components
- Collapsible cards with `data-bs-toggle="collapse"` → Replace with custom accordion/disclosure pattern

**Buttons**
- `.btn`, `.btn-primary`, `.btn-secondary`, `.btn-success`, `.btn-danger`, `.btn-warning` → Replace with `.modern-btn-*`
- `.btn-sm`, `.btn-lg` → Replace with `.modern-btn-small`, `.modern-btn-large`

**Forms**
- `.form-control`, `.form-group`, `.form-check`, `.form-check-input`, `.form-check-label` → Replace with `.modern-form-*` classes
- `.input-group` → Replace with `.modern-input-group`
- `.form-select` → Replace with custom select or `.modern-select`

**Alerts & Notifications**
- `.alert`, `.alert-success`, `.alert-danger`, `.alert-warning`, `.alert-info` → Replace with `.modern-notification-*`, `.modern-info-box`, `.modern-warning-box`, `.modern-success-box`
- `.alert-dismissible` → Replace with custom dismissible notification

**Modals**
- `.modal`, `.modal-dialog`, `.modal-content`, `.modal-header`, `.modal-body`, `.modal-footer` → Replace with custom `.modern-modal` system
- `data-bs-toggle="modal"`, `data-bs-dismiss="modal"` → Replace with JavaScript modal API

**Tables**
- `.table`, `.table-striped`, `.table-hover`, `.table-responsive` → Replace with `.modern-table`, `.modern-table-container`

**Utilities**
- `.mb-3`, `.mt-3`, `.p-3`, etc. → Replace with custom utility classes or CSS variables
- `.text-center`, `.text-white`, `.text-dark` → Replace with custom utilities
- `.d-flex`, `.justify-content-*`, `.align-items-*` → Keep (native CSS, not Bootstrap-specific)

**Navigation**
- `.navbar`, `.nav`, `.nav-item`, `.nav-link` → Already replaced in sidebar

**Progress Bars**
- `.progress`, `.progress-bar` → Replace with `.modern-progress` (if used)

**Badges & Pills**
- `.badge`, `.badge-primary` → Replace with `.modern-badge`

**Dropdowns**
- `.dropdown`, `.dropdown-menu`, `.dropdown-item` → Replace with custom `.modern-dropdown`

**Spinners/Loading**
- `.spinner-border` → Replace with `.modern-spinner`

### Replacement Strategy

#### Phase 1: Create Modern Component CSS Library
**File**: `src/wwwroot/css/modern-components.css`

Create comprehensive modern component system covering all Bootstrap use cases:

```css
/* Modern Container System */
.modern-container {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 24px;
}

.modern-container-fluid {
    width: 100%;
    padding: 0 24px;
}

/* Modern Section/Card System */
.modern-section {
    background: var(--color-bg-secondary);
    border: 1px solid var(--color-border);
    border-radius: 8px;
    padding: 24px;
    margin-bottom: 24px;
}

.modern-section-header {
    font-size: 18px;
    font-weight: 600;
    color: var(--color-text-primary);
    margin-bottom: 16px;
    padding-bottom: 12px;
    border-bottom: 1px solid var(--color-border);
}

.modern-section-body {
    color: var(--color-text-primary);
}

/* Modern Collapsible/Accordion System (replaces Bootstrap collapse cards) */
.modern-collapsible-section {
    background: var(--color-bg-secondary);
    border: 1px solid var(--color-border);
    border-radius: 8px;
    margin-bottom: 16px;
    overflow: hidden;
}

.modern-collapsible-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    background: var(--color-bg-secondary);
    border: none;
    cursor: pointer;
    transition: background-color 0.15s ease;
    user-select: none;
}

.modern-collapsible-header:hover {
    background: var(--color-hover-bg);
}

.modern-collapsible-header.primary {
    background: linear-gradient(135deg, var(--color-azure-blue) 0%, #005a9e 100%);
    color: white;
}

.modern-collapsible-header.secondary {
    background: linear-gradient(135deg, #4a5568 0%, #2d3748 100%);
    color: white;
}

.modern-collapsible-body {
    padding: 20px;
    border-top: 1px solid var(--color-border);
}

.modern-collapsible-body.collapse {
    display: none;
}

.modern-collapsible-body.collapse.show {
    display: block;
}

/* Chevron rotation for collapsed state */
.modern-collapsible-header[aria-expanded="false"] .header-icon {
    transform: rotate(0deg);
    transition: transform 0.2s ease;
}

.modern-collapsible-header[aria-expanded="true"] .header-icon {
    transform: rotate(180deg);
    transition: transform 0.2s ease;
}

/* Modern Button System */
.modern-btn {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 10px 20px;
    border: none;
    border-radius: 6px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.15s ease;
}

.modern-btn-primary {
    background: linear-gradient(135deg, var(--color-azure-blue) 0%, #005a9e 100%);
    color: white;
}

.modern-btn-primary:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(0, 120, 212, 0.3);
}

.modern-btn-secondary {
    background: #f3f4f6;
    color: var(--color-text-primary);
    border: 1px solid var(--color-border);
}

.modern-btn-success {
    background: linear-gradient(135deg, #059669 0%, #047857 100%);
    color: white;
}

.modern-btn-danger {
    background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
    color: white;
}

.modern-btn-warning {
    background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
    color: white;
}

/* Modern Form System */
.modern-form-group {
    margin-bottom: 20px;
}

.modern-form-label {
    display: block;
    font-size: 14px;
    font-weight: 500;
    color: var(--color-text-primary);
    margin-bottom: 8px;
}

.modern-form-input {
    width: 100%;
    padding: 10px 12px;
    border: 1px solid var(--color-border);
    border-radius: 6px;
    font-size: 14px;
    color: var(--color-text-primary);
    background: var(--color-bg-secondary);
    transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.modern-form-input:focus {
    outline: none;
    border-color: var(--color-azure-blue);
    box-shadow: 0 0 0 3px rgba(0, 120, 212, 0.1);
}

.modern-form-select {
    width: 100%;
    padding: 10px 12px;
    border: 1px solid var(--color-border);
    border-radius: 6px;
    font-size: 14px;
    background: var(--color-bg-secondary);
    cursor: pointer;
}

/* Modern Checkbox/Radio */
.modern-form-check {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 12px;
}

.modern-form-check-input {
    width: 18px;
    height: 18px;
    cursor: pointer;
}

.modern-form-check-label {
    font-size: 14px;
    color: var(--color-text-primary);
    cursor: pointer;
}

/* Modern Alert/Notification Boxes */
.modern-notification {
    padding: 16px;
    border-radius: 6px;
    margin-bottom: 16px;
    border-left: 4px solid;
}

.modern-notification-info {
    background: #eff6ff;
    border-left-color: #0078d4;
    color: #1e3a8a;
}

.modern-notification-success {
    background: #f0fdf4;
    border-left-color: #059669;
    color: #14532d;
}

.modern-notification-warning {
    background: #fffbeb;
    border-left-color: #f59e0b;
    color: #78350f;
}

.modern-notification-danger {
    background: #fef2f2;
    border-left-color: #dc2626;
    color: #7f1d1d;
}

/* Modern Info/Warning/Success Boxes (for Admin settings) */
.modern-info-box {
    background: var(--color-info-bg, #eff6ff);
    border: 1px solid var(--color-info-border, #bfdbfe);
    border-radius: 6px;
    padding: 16px;
    color: var(--color-info-text, #1e40af);
}

.modern-warning-box {
    background: var(--color-warning-bg, #fffbeb);
    border: 1px solid var(--color-warning-border, #fde68a);
    border-radius: 6px;
    padding: 16px;
    color: var(--color-warning-text, #78350f);
}

.modern-success-box {
    background: var(--color-success-bg, #f0fdf4);
    border: 1px solid var(--color-success-border, #86efac);
    border-radius: 6px;
    padding: 16px;
    color: var(--color-success-text, #14532d);
}

/* Modern Table System */
.modern-table-container {
    overflow-x: auto;
    border: 1px solid var(--color-border);
    border-radius: 8px;
    margin-bottom: 24px;
}

.modern-table {
    width: 100%;
    border-collapse: collapse;
}

.modern-table thead {
    background: var(--color-bg-primary);
    border-bottom: 2px solid var(--color-border);
}

.modern-table th {
    padding: 12px 16px;
    text-align: left;
    font-weight: 600;
    font-size: 14px;
    color: var(--color-text-primary);
}

.modern-table td {
    padding: 12px 16px;
    border-top: 1px solid var(--color-border);
    font-size: 14px;
    color: var(--color-text-primary);
}

.modern-table tbody tr:hover {
    background: var(--color-hover-bg);
}

/* Modern Modal System */
.modern-modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    z-index: 1040;
    display: none;
}

.modern-modal-backdrop.show {
    display: block;
}

.modern-modal {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background: var(--color-bg-secondary);
    border-radius: 8px;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    max-width: 600px;
    width: 90%;
    max-height: 90vh;
    overflow: hidden;
    z-index: 1050;
    display: none;
}

.modern-modal.show {
    display: block;
}

.modern-modal-header {
    padding: 20px 24px;
    border-bottom: 1px solid var(--color-border);
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.modern-modal-title {
    font-size: 20px;
    font-weight: 600;
    color: var(--color-text-primary);
}

.modern-modal-body {
    padding: 24px;
    overflow-y: auto;
    max-height: calc(90vh - 140px);
}

.modern-modal-footer {
    padding: 16px 24px;
    border-top: 1px solid var(--color-border);
    display: flex;
    gap: 12px;
    justify-content: flex-end;
}

/* Modern Spinner/Loading */
.modern-spinner {
    width: 40px;
    height: 40px;
    border: 4px solid var(--color-border);
    border-top-color: var(--color-azure-blue);
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}

/* Modern Badge System */
.modern-badge {
    display: inline-block;
    padding: 4px 10px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 500;
    line-height: 1;
}

.modern-badge-primary {
    background: #dbeafe;
    color: #1e40af;
}

.modern-badge-success {
    background: #d1fae5;
    color: #065f46;
}

.modern-badge-warning {
    background: #fef3c7;
    color: #78350f;
}

.modern-badge-danger {
    background: #fee2e2;
    color: #7f1d1d;
}

/* Modern Utility Classes */
.modern-mb-1 { margin-bottom: 8px; }
.modern-mb-2 { margin-bottom: 16px; }
.modern-mb-3 { margin-bottom: 24px; }
.modern-mb-4 { margin-bottom: 32px; }

.modern-mt-1 { margin-top: 8px; }
.modern-mt-2 { margin-top: 16px; }
.modern-mt-3 { margin-top: 24px; }
.modern-mt-4 { margin-top: 32px; }

.modern-p-1 { padding: 8px; }
.modern-p-2 { padding: 16px; }
.modern-p-3 { padding: 24px; }
.modern-p-4 { padding: 32px; }

.modern-text-center { text-align: center; }
.modern-text-right { text-align: right; }
.modern-text-left { text-align: left; }

.modern-flex { display: flex; }
.modern-flex-column { flex-direction: column; }
.modern-items-center { align-items: center; }
.modern-justify-between { justify-content: space-between; }
.modern-justify-center { justify-content: center; }
.modern-gap-2 { gap: 16px; }
.modern-gap-3 { gap: 24px; }
```

#### Phase 2: JavaScript for Interactive Components
**File**: `src/wwwroot/js/modern-components.js`

Create JavaScript to handle interactive components (modals, collapsibles, etc.):

```javascript
// Modern Collapsible/Accordion Handler
document.addEventListener('DOMContentLoaded', function() {
    // Handle collapsible sections
    document.querySelectorAll('[data-bs-toggle="collapse"]').forEach(function(trigger) {
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(trigger.getAttribute('href'));
            const isExpanded = trigger.getAttribute('aria-expanded') === 'true';
            
            // Toggle collapsed state
            target.classList.toggle('show');
            trigger.setAttribute('aria-expanded', !isExpanded);
        });
    });
    
    // Modern Modal Handler
    window.ModernModal = {
        show: function(modalId) {
            const modal = document.getElementById(modalId);
            const backdrop = document.createElement('div');
            backdrop.className = 'modern-modal-backdrop show';
            backdrop.id = modalId + '-backdrop';
            document.body.appendChild(backdrop);
            modal.classList.add('show');
            document.body.style.overflow = 'hidden';
        },
        hide: function(modalId) {
            const modal = document.getElementById(modalId);
            const backdrop = document.getElementById(modalId + '-backdrop');
            modal.classList.remove('show');
            if (backdrop) backdrop.remove();
            document.body.style.overflow = '';
        }
    };
});
```

#### Phase 3: Remove Bootstrap References
**Files to Update**:
- `src/App.razor` or `src/Components/_Imports.razor`
- `src/Components/Layout/MainLayout.razor`
- `src/wwwroot/index.html` or equivalent

**Actions**:
1. Remove Bootstrap CSS link: `<link href="bootstrap/bootstrap.min.css" rel="stylesheet" />`
2. Remove Bootstrap JS: `<script src="bootstrap/bootstrap.bundle.min.js"></script>`
3. Add modern component CSS: `<link href="css/modern-components.css" rel="stylesheet" />`
4. Add modern component JS: `<script src="js/modern-components.js"></script>`

#### Phase 4: Page-by-Page Migration

**Migration Order** (by complexity and impact):

1. **Admin.razor** ✅ (COMPLETED - cb93a3e)
   - Status: Fully modernized with collapsible sections
   - Components used: `modern-collapsible-section`, `modern-setting-row`, `modern-form-input`, `modern-info-box`

2. **Configuration.razor** (HIGH PRIORITY)
   - Replace ~50+ Bootstrap collapse cards with modern collapsible sections
   - Pattern: Resource Type cards, Location cards, Environment cards, etc.
   - Estimated time: 4-6 hours

3. **Generate.razor** (HIGH PRIORITY)
   - Replace collapsible instruction cards
   - Replace form controls with modern inputs
   - Update component selection UI
   - Estimated time: 3-4 hours

4. **Reference.razor** (MEDIUM PRIORITY)
   - Replace documentation cards
   - Update code example styling
   - Estimated time: 2-3 hours

5. **Instructions.razor** (MEDIUM PRIORITY)
   - Replace instruction cards
   - Update step-by-step guides
   - Estimated time: 2-3 hours

6. **GeneratedNamesLog.razor** (LOW PRIORITY)
   - Replace table Bootstrap classes
   - Update filter/search inputs
   - Estimated time: 2-3 hours

7. **AdminLog.razor** (LOW PRIORITY)
   - Similar to GeneratedNamesLog
   - Estimated time: 1-2 hours

8. **Resource Component Pages** (MEDIUM PRIORITY)
   - Multiple files (ResourceTypes, Locations, Environments, etc.)
   - All follow same pattern - can use template approach
   - Estimated time: 4-6 hours total

### Migration Pattern Template

**Old Bootstrap Pattern**:
```razor
<div class="card mb-3" style="width:auto;">
    <div class="card-header bg-default text-dark">
        <a class="text-decoration-none text-dark" data-bs-toggle="collapse" 
           href="#sectionId" role="button" aria-expanded="false">
            <span class="oi oi-chevron-bottom"></span> Section Title
        </a>
    </div>
    <div class="collapse show card card-body @theme.ThemeStyle" id="sectionId">
        <p>Section content...</p>
        <button class="btn btn-primary">Action</button>
    </div>
</div>
```

**New Modern Pattern**:
```razor
<div class="modern-collapsible-section">
    <div class="modern-collapsible-header secondary" data-bs-toggle="collapse" 
         href="#sectionId" role="button" aria-expanded="false">
        <div class="header-title">
            <span class="oi oi-cog" aria-hidden="true"></span>
            <span>Section Title</span>
        </div>
        <span class="oi oi-chevron-bottom header-icon" aria-hidden="true"></span>
    </div>
    <div class="collapse modern-collapsible-body" id="sectionId">
        <p class="text-secondary">Section content...</p>
        <button class="modern-btn-primary">Action</button>
    </div>
</div>
```

### Testing Strategy

1. **Visual Testing**: Test each page after migration in multiple browsers
2. **Functional Testing**: Verify all collapse/expand, form submissions, modals work
3. **Responsive Testing**: Test mobile, tablet, desktop layouts
4. **Accessibility Testing**: Verify keyboard navigation, screen reader compatibility
5. **Theme Testing**: Verify light/dark mode works correctly with new components
6. **Performance Testing**: Measure page load time improvement after Bootstrap removal

### Rollback Safety

- **Git Commits**: Commit after each page migration for easy rollback
- **Feature Flag**: Consider temporary feature flag to toggle between Bootstrap and modern design
- **Documentation**: Document all component replacements in this file

### Time Estimates

| Task | Estimated Time |
|------|----------------|
| Create modern-components.css | 3-4 hours |
| Create modern-components.js | 2-3 hours |
| Remove Bootstrap references | 1 hour |
| Migrate Configuration.razor | 4-6 hours |
| Migrate Generate.razor | 3-4 hours |
| Migrate Reference.razor | 2-3 hours |
| Migrate Instructions.razor | 2-3 hours |
| Migrate GeneratedNamesLog.razor | 2-3 hours |
| Migrate AdminLog.razor | 1-2 hours |
| Migrate Resource Component Pages | 4-6 hours |
| Testing & Bug Fixes | 4-6 hours |
| **TOTAL** | **28-41 hours** |

---

## Implementation Phases

### Phase 1: Foundation Setup (Est. 7-10 hours - Updated)
**Goal**: Create shared CSS foundation, update layout structure, and implement theme system

#### 1.1: Create Global CSS File
- **File**: `src/wwwroot/css/modern-design.css`
- **Content**:
  - CSS variables for all colors
  - Typography system
  - Spacing utilities
  - Common component styles
  - Animation/transition definitions
- **Action Items**:
  - [ ] Create new CSS file with design system variables
  - [ ] Add CSS file reference to `App.razor` or `_Imports.razor`
  - [ ] Test CSS loading in development environment

#### 1.2: Update Main Layout Structure
- **Files**: 
  - `src/Components/Layout/MainLayout.razor`
  - `src/Components/Layout/NavMenu.razor`
- **Changes**:
  - Replace existing layout structure with DesignLinear layout
  - Implement dark gray sidebar navigation
  - Add collapsible sidebar functionality
  - Update logo/header section
- **Action Items**:
  - [ ] Backup existing `MainLayout.razor`
  - [ ] Implement new sidebar structure from DesignLinear
  - [ ] Add JavaScript for sidebar collapse toggle
  - [ ] Update navigation menu items with new styling
  - [ ] Test responsive behavior (mobile/tablet/desktop)

#### 1.3: Update Navigation Menu
- **File**: `src/Components/Layout/NavMenu.razor`
- **Changes**:
  - Apply dark gray background gradient
  - White text with transparency
  - Add hover/active states
  - Update section dividers
  - Add section labels (Main, Components, System)
- **Action Items**:
  - [ ] Update navigation item structure
  - [ ] Apply new color scheme
  - [ ] Add icons to navigation items (if not present)
  - [ ] Test navigation highlighting for active pages
  - [ ] Verify collapse/expand functionality

---

### Phase 2: Component Library Updates (Est. 6-8 hours)
**Goal**: Modernize all reusable UI components

#### 2.1: Update Card Components
- **Files**: All pages using card layouts
- **Changes**:
  - Replace Bootstrap cards with new card styling
  - Apply border: `1px solid #e5e5e5`
  - Border radius: `8px`
  - Hover effects with Azure blue borders
  - Remove Bootstrap classes (`card`, `card-body`, etc.)
- **Action Items**:
  - [ ] Identify all card usages across application
  - [ ] Create reusable card component (optional)
  - [ ] Update card styling site-wide
  - [ ] Test card hover effects

#### 2.2: Update Button Components
- **Changes**:
  - **Primary Buttons**: Azure blue gradient (`#0078d4` → `#005a9e`)
  - **Secondary Buttons**: White with gray border
  - **Danger Buttons**: Red (keep existing, adjust to match style)
  - Remove Bootstrap button classes
  - Add consistent padding: `10px 20px`
  - Add hover lift effect: `translateY(-1px)`
  - Add shadow: `0 2px 8px rgba(0, 120, 212, 0.2)`
- **Action Items**:
  - [ ] Audit all button usages
  - [ ] Create button CSS classes (`.btn-primary-modern`, `.btn-secondary-modern`)
  - [ ] Replace Bootstrap button classes
  - [ ] Test button states (normal, hover, active, disabled)

#### 2.3: Update Form Components
- **Elements**: Inputs, Selects, Textareas, Checkboxes, Radio buttons
- **Changes**:
  - Remove Bootstrap form classes
  - Border: `1px solid #e5e5e5`
  - Border radius: `6px`
  - Focus state: Azure blue border (`#0078d4`)
  - Background: `#fafafa` (unfocused), `#ffffff` (focused)
  - Padding: `8px 12px`
- **Action Items**:
  - [ ] Create form input CSS classes
  - [ ] Update all input fields
  - [ ] Update select dropdowns
  - [ ] Update textareas
  - [ ] Update checkboxes/radio buttons styling
  - [ ] Test form validation styling
  - [ ] Test accessibility (focus indicators, labels)

#### 2.4: Update Tables
- **Changes**:
  - Remove Bootstrap table classes
  - Clean borders: `1px solid #e5e5e5`
  - Header background: `#f8f9fa` or light gray
  - Row hover: `#fafafa`
  - Padding: `12px 16px`
  - Remove striped backgrounds (optional)
- **Action Items**:
  - [ ] Identify all table usages
  - [ ] Create table CSS classes
  - [ ] Update table styling
  - [ ] Test responsive table behavior

---

### Phase 3: Page Updates (Est. 8-10 hours)
**Goal**: Update all application pages with new design

#### 3.1: Dashboard/Home Page
- **File**: `src/Components/Pages/Home.razor` (or main dashboard)
- **Changes**:
  - Add stats grid with teal values (`#00b7c3`)
  - Action cards with Azure blue hover
  - Recent activity section with purple icons
  - Update page title with Azure blue color
- **Action Items**:
  - [ ] Update page header/title styling
  - [ ] Implement stats card grid
  - [ ] Update action cards
  - [ ] Add/update activity list
  - [ ] Test responsive layout

#### 3.2: Configuration Page
- **File**: `src/Components/Pages/Configuration.razor`
- **Changes**:
  - Update section headers with bright cyan (`#50e6ff`)
  - Update import/export cards
  - Update backup/restore UI
  - Apply new button styling
- **Action Items**:
  - [ ] Update page layout
  - [ ] Update section styling
  - [ ] Update card components
  - [ ] Update buttons
  - [ ] Test backup/restore functionality with new design

#### 3.3: Admin Page
- **File**: `src/Components/Pages/Admin.razor`
- **Changes**:
  - Update migration section
  - Update settings sections
  - Apply new form styling
  - Update status indicators
- **Action Items**:
  - [ ] Update admin sections
  - [ ] Update form inputs
  - [ ] Update status displays
  - [ ] Test migration workflow with new design

#### 3.4: Resource Component Pages
- **Files**: 
  - `ResourceTypes.razor`
  - `ResourceLocations.razor`
  - `ResourceEnvironments.razor`
  - `ResourceOrgs.razor`
  - `ResourceProjAppSvcs.razor`
  - `ResourceUnitDepts.razor`
  - `ResourceFunctions.razor`
  - `CustomComponents.razor`
- **Changes**: Apply consistent styling to all component pages
- **Action Items**:
  - [ ] Update page headers
  - [ ] Update data tables
  - [ ] Update action buttons (Add, Edit, Delete)
  - [ ] Update search/filter inputs
  - [ ] Test CRUD operations with new design

#### 3.5: Generate Name Page
- **File**: `src/Components/Pages/ResourceNaming.razor` (or similar)
- **Changes**:
  - Update name generation form
  - Update preview/result display
  - Update component selection dropdowns
- **Action Items**:
  - [ ] Update form layout
  - [ ] Update result display
  - [ ] Update validation styling
  - [ ] Test name generation workflow

#### 3.6: Reference Page
- **File**: Reference/documentation pages
- **Changes**:
  - Update documentation display
  - Update code examples styling
  - Update navigation/breadcrumbs
- **Action Items**:
  - [ ] Update documentation layout
  - [ ] Update code snippet styling
  - [ ] Update navigation elements

---

### Phase 4: Modal & Notification Updates (Est. 4-6 hours)
**Goal**: Modernize all modals and notification components

#### 4.1: Update Modal Components
- **Files**: 
  - `src/Components/Modals/*.razor`
  - `TextConfirmationModal.razor`
  - Any other modal components
- **Changes**:
  - Update modal backdrop: `rgba(0, 0, 0, 0.5)`
  - Update modal container styling
  - Border radius: `8px`
  - Remove Bootstrap modal classes
  - Update modal header (close button, title)
  - Update modal footer buttons
  - Add subtle shadow: `0 10px 40px rgba(0, 0, 0, 0.2)`
- **Action Items**:
  - [ ] Identify all modal components
  - [ ] Create modal CSS base classes
  - [ ] Update each modal component
  - [ ] Update modal animations (fade in/out)
  - [ ] Test modal open/close behavior
  - [ ] Test modal backdrop click handling
  - [ ] Test TextConfirmationModal specifically

#### 4.2: Update Notification/Toast Components
- **Files**: Notification/toast components
- **Changes**:
  - Position: top-right or top-center
  - Background colors:
    - Success: Light green with green border/icon
    - Error: Light red with red border/icon
    - Warning: Light yellow with yellow border/icon
    - Info: Light blue with blue border/icon
  - Border radius: `6px`
  - Shadow: `0 4px 12px rgba(0, 0, 0, 0.15)`
  - Slide-in animation from right
- **Action Items**:
  - [ ] Update notification styling
  - [ ] Update notification icons
  - [ ] Update notification animations
  - [ ] Test notification timing/auto-dismiss
  - [ ] Test multiple notifications stacking

#### 4.3: Update Confirmation Dialogs
- **Changes**:
  - Update styling to match modal design
  - Azure blue primary button
  - Gray secondary button
- **Action Items**:
  - [ ] Update confirmation dialog styling
  - [ ] Test confirmation workflows

---

### Phase 5: Header & Top Bar Updates (Est. 2-3 hours)
**Goal**: Modernize top navigation and header elements

#### 5.1: Update Top Bar
- **File**: `MainLayout.razor` or header component
- **Changes**:
  - Height: `64px`
  - Background: `#ffffff`
  - Border bottom: `1px solid #e5e5e5`
  - Update search box styling (if present)
  - Update user menu/settings icons
  - Update breadcrumbs (if present)
- **Action Items**:
  - [ ] Update header layout
  - [ ] Update search functionality styling
  - [ ] Update icon buttons
  - [ ] Update user menu dropdown
  - [ ] Test responsive behavior

#### 5.2: Update Page Headers
- **Changes**:
  - Page title: Azure blue color (`#0078d4`)
  - Subtitle: Gray (`#737373`)
  - Breadcrumbs: Azure blue links
  - Action buttons: Align right
- **Action Items**:
  - [ ] Create page header component/template
  - [ ] Apply to all pages
  - [ ] Test header consistency

---

### Phase 6: Responsive & Mobile Updates (Est. 4-6 hours)
**Goal**: Ensure design works across all screen sizes

#### 6.1: Mobile Sidebar
- **Changes**:
  - Sidebar: overlay on mobile (< 768px)
  - Add mobile menu toggle button
  - Backdrop when sidebar open
  - Slide-in animation
- **Action Items**:
  - [ ] Implement mobile sidebar overlay
  - [ ] Add hamburger menu button
  - [ ] Add backdrop click to close
  - [ ] Test on mobile devices/emulators

#### 6.2: Responsive Grid Adjustments
- **Changes**:
  - Stats grid: 1 column on mobile
  - Action cards: 1 column on mobile
  - Tables: horizontal scroll or stacked layout
  - Forms: full width on mobile
- **Action Items**:
  - [ ] Test all pages on mobile (< 768px)
  - [ ] Test on tablet (768px - 1024px)
  - [ ] Adjust breakpoints as needed
  - [ ] Test form usability on mobile

#### 6.3: Touch Interactions
- **Changes**:
  - Increase button/link tap targets (min 44px)
  - Test touch gestures (swipe, tap)
  - Test modal interactions on touch devices
- **Action Items**:
  - [ ] Test on actual touch devices
  - [ ] Adjust tap target sizes
  - [ ] Test swipe gestures if applicable

---

### Phase 7: Polish & Refinement (Est. 3-4 hours)
**Goal**: Final polish and consistency checks

#### 7.1: Consistency Audit
- **Action Items**:
  - [ ] Audit all pages for consistent spacing
  - [ ] Audit all pages for consistent colors
  - [ ] Audit all pages for consistent typography
  - [ ] Audit all buttons for consistent styling
  - [ ] Audit all forms for consistent styling
  - [ ] Audit all tables for consistent styling
  - [ ] Check for any remaining Bootstrap classes

#### 7.2: Animation & Transitions
- **Action Items**:
  - [ ] Verify all hover transitions (`0.15s ease`)
  - [ ] Verify button hover effects
  - [ ] Verify card hover effects
  - [ ] Verify modal animations
  - [ ] Verify notification animations
  - [ ] Ensure no janky animations

#### 7.3: Accessibility Check
- **Action Items**:
  - [ ] Test keyboard navigation (Tab, Enter, Esc)
  - [ ] Test focus indicators visibility
  - [ ] Test color contrast (WCAG AA minimum)
  - [ ] Test with screen reader (basic check)
  - [ ] Verify form labels and ARIA attributes
  - [ ] Verify button/link accessible names

#### 7.4: Cross-Browser Testing
- **Action Items**:
  - [ ] Test on Chrome/Edge (Chromium)
  - [ ] Test on Firefox
  - [ ] Test on Safari (Mac/iOS)
  - [ ] Test on mobile browsers (Chrome Mobile, Safari Mobile)
  - [ ] Fix any browser-specific issues

---

### Phase 8: Cleanup & Documentation (Est. 2-3 hours)
**Goal**: Remove old code and document changes

#### 8.1: Remove Bootstrap Dependencies
- **Action Items**:
  - [ ] Remove Bootstrap CSS references
  - [ ] Remove unused Bootstrap JavaScript
  - [ ] Remove Bootstrap class usage (search for `class="btn `, `class="card `, etc.)
  - [ ] Update package.json if Bootstrap was a dependency
  - [ ] Test application after Bootstrap removal

#### 8.2: Remove Design Mockups
- **Action Items**:
  - [ ] Delete `src/Components/Pages/DesignMockups/` folder
  - [ ] Remove mockup routes
  - [ ] Clean up any mockup-related code

#### 8.3: Update Documentation
- **Files**: 
  - `README.md`
  - Any style guide or contributing docs
- **Action Items**:
  - [ ] Document new color palette (light and dark modes)
  - [ ] Document component styling patterns
  - [ ] Document theme toggle usage
  - [ ] Update screenshots/demos if applicable (show both themes)
  - [ ] Add design system reference

#### 8.4: Theme System Documentation
- **Action Items**:
  - [ ] Document CSS variable naming conventions
  - [ ] Document how to add new theme-aware components
  - [ ] Document localStorage theme persistence
  - [ ] Document system preference detection (if implemented)
  - [ ] Create guide for future theme customization

#### 8.4: Performance Optimization
- **Action Items**:
  - [ ] Minify CSS in production
  - [ ] Check for unused CSS
  - [ ] Verify bundle size hasn't increased significantly
  - [ ] Test page load performance

---

## Testing Strategy

### Functional Testing
- [ ] Test all existing functionality works with new design
- [ ] Test all forms submit correctly
- [ ] Test all CRUD operations
- [ ] Test name generation workflow
- [ ] Test configuration import/export
- [ ] Test backup/restore functionality
- [ ] Test migration workflow
- [ ] Test admin features

### Visual Testing
- [ ] Compare pages to DesignLinear mockup
- [ ] Verify color consistency
- [ ] Verify spacing consistency
- [ ] Verify typography consistency
- [ ] Take screenshots for documentation

### Responsive Testing
- [ ] Test on mobile devices (< 768px)
- [ ] Test on tablets (768px - 1024px)
- [ ] Test on desktop (> 1024px)
- [ ] Test on large screens (> 1920px)

### Browser Testing
- [ ] Chrome/Edge (Windows)
- [ ] Firefox (Windows)
- [ ] Safari (Mac/iOS)
- [ ] Mobile browsers

### Accessibility Testing
- [ ] Keyboard navigation
- [ ] Screen reader compatibility (basic)
- [ ] Color contrast
- [ ] Focus indicators

---

## Risk Assessment

### High Risk Items
1. **Bootstrap Removal**: May break existing functionality if not carefully removed
   - **Mitigation**: Test thoroughly after removal, keep Bootstrap until end
2. **Layout Changes**: Major layout changes could affect existing functionality
   - **Mitigation**: Test all pages after layout updates
3. **Form Styling**: Custom form styling could break validation displays
   - **Mitigation**: Test all forms with validation errors

### Medium Risk Items
1. **Modal Updates**: Could affect modal open/close behavior
   - **Mitigation**: Test all modals thoroughly
2. **Responsive Behavior**: Mobile layout could be problematic
   - **Mitigation**: Test on actual devices early
3. **Color Contrast**: New colors might not meet accessibility standards
   - **Mitigation**: Check WCAG contrast ratios

### Low Risk Items
1. **Button Styling**: Isolated changes, easy to test
2. **Typography**: Low impact on functionality
3. **Animations**: Can be disabled if problematic

---

## Rollback Plan

### If Critical Issues Found
1. Keep a branch with current Bootstrap design: `backup/bootstrap-design`
2. Git commits should be incremental and well-documented
3. Each phase should be committable separately
4. If rollback needed, can revert specific commits

### Rollback Triggers
- Broken functionality that can't be quickly fixed
- Severe accessibility issues
- Critical browser incompatibilities
- Performance degradation

---

## Timeline Estimate

### Total Estimated Time: 33-46 hours

| Phase | Estimated Time | Priority |
|-------|---------------|----------|
| Phase 1: Foundation Setup (includes Theme System) | 7-10 hours | CRITICAL |
| Phase 2: Component Library Updates | 6-8 hours | HIGH |
| Phase 3: Page Updates | 8-10 hours | HIGH |
| Phase 4: Modal & Notification Updates | 4-6 hours | MEDIUM |
| Phase 5: Header & Top Bar Updates (includes Theme Toggle) | 4-6 hours | MEDIUM |
| Phase 6: Responsive & Mobile Updates | 4-6 hours | HIGH |
| Phase 7: Polish & Refinement | 3-4 hours | MEDIUM |
| Phase 8: Cleanup & Documentation (includes Theme Docs) | 2-3 hours | LOW |
| **TOTAL** | **39-55 hours** | |

### Suggested Schedule
- **Week 1**: Phases 1-2 (Foundation + Theme System + Components)
- **Week 2**: Phase 3 (Page Updates)
- **Week 3**: Phases 4-5 (Modals + Header + Theme Toggle)
- **Week 4**: Phases 6-8 (Responsive + Polish + Cleanup + Theme Testing)

---

## Success Criteria

### Must Have
- [ ] All pages render correctly with new design
- [ ] All functionality works as before
- [ ] Mobile-responsive design works
- [ ] No console errors
- [ ] Meets WCAG AA color contrast standards
- [ ] Works in Chrome, Firefox, Safari

### Nice to Have
- [ ] Smooth animations throughout
- [ ] Improved performance over Bootstrap version
- [ ] Enhanced mobile experience
- [ ] Better accessibility than before

---

## Notes

### Design System Files to Create
1. `src/wwwroot/css/modern-design.css` - Main design system CSS
2. `src/wwwroot/css/modern-components.css` - Component-specific styles
3. `src/wwwroot/css/modern-responsive.css` - Responsive/mobile styles

### CSS Variables to Define
```css
:root {
    /* Sidebar */
    --sidebar-bg-start: #2d3748;
    --sidebar-bg-end: #1a202c;
    --sidebar-text: rgba(255, 255, 255, 0.85);
    --sidebar-text-active: #ffffff;
    --sidebar-hover: rgba(255, 255, 255, 0.1);
    
    /* Main Content */
    --bg-body: #fafafa;
    --bg-card: #ffffff;
    --border-color: #e5e5e5;
    --text-primary: #171717;
    --text-secondary: #737373;
    
    /* Azure Accents */
    --azure-blue: #0078d4;
    --azure-blue-dark: #005a9e;
    --azure-teal: #00b7c3;
    --azure-cyan: #50e6ff;
    --azure-purple: #8661c5;
    --success-green: #059669;
    
    /* Spacing */
    --spacing-xs: 4px;
    --spacing-sm: 8px;
    --spacing-md: 16px;
    --spacing-lg: 24px;
    --spacing-xl: 32px;
    
    /* Border Radius */
    --radius-sm: 6px;
    --radius-md: 8px;
    
    /* Transitions */
    --transition-fast: 0.15s ease;
}
```

### Key Decisions Made
1. **Dark Sidebar**: Provides professional look and contrasts with content
2. **Azure Colors**: Aligns with Azure branding, modern and recognizable
3. **Multiple Accent Colors**: Prevents monotony, creates visual hierarchy
4. **Minimal Shadows**: Clean, flat design with subtle depth
5. **System Fonts**: Fast loading, native feel

---

## Appendix: File Inventory

### Files to Create (NEW)
- `src/wwwroot/css/modern-design.css` - Main design system CSS with theme variables
- `src/wwwroot/css/modern-components.css` - Component-specific styles
- `src/wwwroot/css/modern-responsive.css` - Responsive/mobile styles
- `src/wwwroot/js/theme.js` - Theme management JavaScript
- `src/Components/General/ThemeToggle.razor` - Theme toggle component

### Files to Modify (Estimated)
- **Layout Components**: 2-3 files
- **Page Components**: 15-20 files
- **Modal Components**: 5-8 files
- **Shared Components**: 5-10 files
- **CSS Files**: Create 3 new files
- **Configuration Files**: 1-2 files

### Files to Create
- `src/wwwroot/css/modern-design.css`
- `src/wwwroot/css/modern-components.css`
- `src/wwwroot/css/modern-responsive.css`

### Files to Delete
- `src/Components/Pages/DesignMockups/` (entire folder after completion)

---

## Status Tracking

### Phase 1: Foundation Setup (includes Theme System)
- [ ] 1.1: Create Global CSS File
- [ ] 1.2: Update Main Layout Structure
- [ ] 1.3: Update Navigation Menu
- [ ] 1.4: Theme System Foundation (NEW)
  - [ ] Create CSS variables for light/dark themes
  - [ ] Add data-theme attribute support
  - [ ] Create theme.js for localStorage management
  - [ ] Add theme transition CSS
  - [ ] Test theme switching

### Phase 2: Component Library Updates
- [ ] 2.1: Update Card Components
- [ ] 2.2: Update Button Components
- [ ] 2.3: Update Form Components
- [ ] 2.4: Update Tables

### Phase 3: Page Updates
- [ ] 3.1: Dashboard/Home Page
- [ ] 3.2: Configuration Page
- [ ] 3.3: Admin Page
- [ ] 3.4: Resource Component Pages
- [ ] 3.5: Generate Name Page
- [ ] 3.6: Reference Page

### Phase 4: Modal & Notification Updates
- [ ] 4.1: Update Modal Components
- [ ] 4.2: Update Notification/Toast Components
- [ ] 4.3: Update Confirmation Dialogs

### Phase 5: Header & Top Bar Updates (includes Theme Toggle)
- [ ] 5.1: Update Top Bar
- [ ] 5.2: Update Page Headers
- [ ] 5.5: Theme Toggle Component (NEW)
  - [ ] Create ThemeToggle.razor component
  - [ ] Add toggle button styling
  - [ ] Integrate into header
  - [ ] Add keyboard accessibility
  - [ ] Test theme persistence

### Phase 6: Responsive & Mobile Updates
- [ ] 6.1: Mobile Sidebar
- [ ] 6.2: Responsive Grid Adjustments
- [ ] 6.3: Touch Interactions

### Phase 7: Polish & Refinement
- [ ] 7.1: Consistency Audit
- [ ] 7.2: Animation & Transitions
- [ ] 7.3: Accessibility Check
- [ ] 7.4: Cross-Browser Testing
- [ ] 7.5: Theme Testing (NEW)
  - [ ] Test all pages in light mode
  - [ ] Test all pages in dark mode
  - [ ] Test theme persistence
  - [ ] Test color contrast (WCAG AA)
  - [ ] Test theme toggle accessibility

### Phase 8: Cleanup & Documentation
- [ ] 8.1: Remove Bootstrap Dependencies
- [ ] 8.2: Remove Design Mockups
- [ ] 8.3: Update Documentation
- [ ] 8.4: Theme System Documentation (NEW)
  - [ ] Document CSS variables
  - [ ] Document theme customization
  - [ ] Document localStorage persistence
  - [ ] Update README with theme screenshots
- [ ] 8.5: Performance Optimization

---

**Last Updated**: October 17, 2025
**Document Version**: 1.1
**Status**: Planning Phase - Theme System Added

---

## Design Implementation Commit Tracking

To ensure easy rollback and traceability, record the commit hashes for the following milestones:

- **PRE-DESIGN COMMIT**: The last commit before any DesignLinear modernization changes were applied.
  - Commit hash: `ae7ef626f7e91efbb50c5918abdc18c5ace522f7`
  - Date: October 17, 2025
  - Description: Last stable commit before design system implementation began.

- **POST-DESIGN COMMIT**: The first commit after the initial DesignLinear modernization changes were applied (foundation CSS, layout, theme system, sidebar styling).
  - Commit hash: `fb7b27b0fba08cefb00309380644084afd3167bb`
  - Date: 2025-10-17 08:19:01 -0500
  - Message: feat: Begin DesignLinear implementation - rollback point
  - Description: Initial commit for DesignLinear modernization (foundation CSS, theme system, MainLayout, NavMenu sidebar styling).

- **DASHBOARD MODERNIZATION COMMIT**: Dashboard page modernized with stats grid, feature list, and modern cards.
  - Commit hash: `4e68208`
  - Date: 2025-10-17
  - Message: feat: modernize Dashboard with stats grid and feature cards
  - Description: Completed Dashboard (Index.razor) modernization with live stats, feature highlights, and modern card design.

- **ADMIN PAGE START COMMIT**: Initial admin page modernization - login card and CSS components.
  - Commit hash: `faae420`
  - Date: 2025-10-17
  - Message: feat: Begin Admin page modernization - login card and CSS components
  - Description: Started Admin.razor modernization with modern login card and admin-specific CSS components.

- **ADMIN PAGE COMPLETE COMMIT**: Completed admin page modernization with all sections.
  - Commit hash: `cb93a3e`
  - Date: 2025-10-17
  - Message: feat: modernize Admin page with DesignLinear - Customization, Site Settings, Security, Identity Provider sections
  - Description: Completed Admin.razor modernization with all major sections: Customization (home content, logo, navigation), Site Settings (storage provider, toggles, webhook), Security (password, API keys), and Identity Provider Settings (header name, admin users). All sections use modern collapsible design, setting rows, and form components.

> Update these hashes after each commit to keep a clear record for rollback and auditing.

