/* ===================================================================
   Azure Naming Tool - Modern Component JavaScript
   Version: 1.0
   Date: October 17, 2025
   Description: JavaScript handlers for modern interactive components
                Replaces Bootstrap's JavaScript functionality
   =================================================================== */

// Initialize all components when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeCollapsibles();
    initializeModals();
    initializeDropdowns();
    initializeDismissibles();
    initializeTabs();
    initializeScrollToTop();
    forceBlazoredModalStyling();
});

/* ===================================================================
   TAB COMPONENTS
   Modern tabbed navigation system
   =================================================================== */

function initializeTabs() {
    // Handle all tab navigation
    document.querySelectorAll('.modern-tab').forEach(function(tab) {
        // Remove existing listeners to prevent duplicates
        const newTab = tab.cloneNode(true);
        tab.parentNode.replaceChild(newTab, tab);
        
        newTab.addEventListener('click', function(e) {
            e.preventDefault();
            
            const targetId = newTab.getAttribute('data-tab');
            if (!targetId) {
                return;
            }
            
            // Get the tab container
            const tabContainer = newTab.closest('.modern-tabs').parentElement;
            
            // Deactivate all tabs in this container
            tabContainer.querySelectorAll('.modern-tab').forEach(function(t) {
                t.classList.remove('active');
            });
            
            // Activate clicked tab
            newTab.classList.add('active');
            
            // Hide all tab contents in this container
            tabContainer.querySelectorAll('.modern-tab-content').forEach(function(content) {
                content.classList.remove('active');
            });
            
            // Show target content
            const targetContent = document.getElementById(targetId);
            if (targetContent) {
                targetContent.classList.add('active');
            }
        });
    });
    
    // Activate first tab by default if none are active
    document.querySelectorAll('.modern-tabs').forEach(function(tabsContainer) {
        const activeTabs = tabsContainer.querySelectorAll('.modern-tab.active');
        if (activeTabs.length === 0) {
            const firstTab = tabsContainer.querySelector('.modern-tab');
            if (firstTab) {
                firstTab.click();
            }
        }
    });
}

// Expose function globally for Blazor
window.initializeTabs = initializeTabs;

/* ===================================================================
   COLLAPSIBLE/ACCORDION COMPONENTS
   Replaces Bootstrap's data-bs-toggle="collapse" functionality
   =================================================================== */

function initializeCollapsibles() {
    // Handle all elements with data-bs-toggle="collapse"
    document.querySelectorAll('[data-bs-toggle="collapse"]').forEach(function(trigger) {
        // Set initial state
        const targetSelector = trigger.getAttribute('href') || trigger.getAttribute('data-bs-target');
        if (!targetSelector) return;
        
        const target = document.querySelector(targetSelector);
        if (!target) return;
        
        // Initialize aria-expanded based on current state
        const isExpanded = target.classList.contains('show');
        trigger.setAttribute('aria-expanded', isExpanded);
        
        // Add click handler
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            toggleCollapse(trigger, target);
        });
    });
}

function toggleCollapse(trigger, target) {
    const isCurrentlyExpanded = trigger.getAttribute('aria-expanded') === 'true';
    
    if (isCurrentlyExpanded) {
        // Collapse
        target.classList.remove('show');
        trigger.setAttribute('aria-expanded', 'false');
        trigger.classList.remove('expanded');
    } else {
        // Expand
        target.classList.add('show');
        trigger.setAttribute('aria-expanded', 'true');
        trigger.classList.add('expanded');
    }
}

/* ===================================================================
   MODAL COMPONENTS
   Replaces Bootstrap's modal functionality
   =================================================================== */

window.ModernModal = {
    /**
     * Show a modal by ID
     * @param {string} modalId - The ID of the modal element
     */
    show: function(modalId) {
        const modal = document.getElementById(modalId);
        if (!modal) {
            console.error(`Modal with ID "${modalId}" not found`);
            return;
        }
        
        // Create backdrop
        const backdrop = document.createElement('div');
        backdrop.className = 'modern-modal-backdrop';
        backdrop.id = modalId + '-backdrop';
        backdrop.addEventListener('click', function() {
            ModernModal.hide(modalId);
        });
        
        document.body.appendChild(backdrop);
        
        // Show modal with animation
        setTimeout(function() {
            backdrop.classList.add('show');
            modal.classList.add('show');
        }, 10);
        
        // Prevent body scroll
        document.body.style.overflow = 'hidden';
        
        // Focus trap
        const focusableElements = modal.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
        if (focusableElements.length > 0) {
            focusableElements[0].focus();
        }
        
        // ESC key handler
        const escHandler = function(e) {
            if (e.key === 'Escape') {
                ModernModal.hide(modalId);
                document.removeEventListener('keydown', escHandler);
            }
        };
        document.addEventListener('keydown', escHandler);
        modal.dataset.escHandler = 'attached';
    },
    
    /**
     * Hide a modal by ID
     * @param {string} modalId - The ID of the modal element
     */
    hide: function(modalId) {
        const modal = document.getElementById(modalId);
        const backdrop = document.getElementById(modalId + '-backdrop');
        
        if (modal) {
            modal.classList.remove('show');
        }
        
        if (backdrop) {
            backdrop.classList.remove('show');
            setTimeout(function() {
                backdrop.remove();
            }, 150); // Match CSS animation duration
        }
        
        // Restore body scroll
        document.body.style.overflow = '';
    },
    
    /**
     * Toggle a modal by ID
     * @param {string} modalId - The ID of the modal element
     */
    toggle: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal && modal.classList.contains('show')) {
            this.hide(modalId);
        } else {
            this.show(modalId);
        }
    }
};

function initializeModals() {
    // Handle all elements with data-bs-toggle="modal"
    document.querySelectorAll('[data-bs-toggle="modal"]').forEach(function(trigger) {
        const targetSelector = trigger.getAttribute('data-bs-target') || trigger.getAttribute('href');
        if (!targetSelector) return;
        
        const modalId = targetSelector.replace('#', '');
        
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            ModernModal.show(modalId);
        });
    });
    
    // Handle all elements with data-bs-dismiss="modal"
    document.querySelectorAll('[data-bs-dismiss="modal"]').forEach(function(trigger) {
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            const modal = trigger.closest('.modern-modal');
            if (modal) {
                ModernModal.hide(modal.id);
            }
        });
    });
}

/* ===================================================================
   DROPDOWN COMPONENTS
   Replaces Bootstrap's dropdown functionality
   =================================================================== */

window.ModernDropdown = {
    /**
     * Toggle a dropdown by ID
     * @param {string} dropdownId - The ID of the dropdown menu element
     */
    toggle: function(dropdownId) {
        const dropdown = document.getElementById(dropdownId);
        if (!dropdown) return;
        
        const isVisible = dropdown.classList.contains('show');
        
        // Close all other dropdowns
        document.querySelectorAll('.modern-dropdown-menu.show').forEach(function(menu) {
            if (menu.id !== dropdownId) {
                menu.classList.remove('show');
            }
        });
        
        // Toggle this dropdown
        if (isVisible) {
            dropdown.classList.remove('show');
        } else {
            dropdown.classList.add('show');
        }
    },
    
    /**
     * Close all dropdowns
     */
    closeAll: function() {
        document.querySelectorAll('.modern-dropdown-menu.show').forEach(function(menu) {
            menu.classList.remove('show');
        });
    }
};

function initializeDropdowns() {
    // Handle all elements with data-bs-toggle="dropdown"
    document.querySelectorAll('[data-bs-toggle="dropdown"]').forEach(function(trigger) {
        const targetSelector = trigger.getAttribute('data-bs-target') || trigger.getAttribute('href');
        if (!targetSelector) return;
        
        const dropdownId = targetSelector.replace('#', '');
        
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            ModernDropdown.toggle(dropdownId);
        });
    });
    
    // Close dropdowns when clicking outside
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.modern-dropdown')) {
            ModernDropdown.closeAll();
        }
    });
}

/* ===================================================================
   DISMISSIBLE COMPONENTS
   Replaces Bootstrap's alert dismissible functionality
   =================================================================== */

function initializeDismissibles() {
    // Handle all elements with data-bs-dismiss attribute
    document.querySelectorAll('[data-bs-dismiss]').forEach(function(trigger) {
        const dismissType = trigger.getAttribute('data-bs-dismiss');
        
        if (dismissType === 'alert' || dismissType === 'notification') {
            trigger.addEventListener('click', function(e) {
                e.preventDefault();
                const parent = trigger.closest('.modern-notification, .modern-alert, .alert');
                if (parent) {
                    parent.style.opacity = '0';
                    parent.style.transform = 'translateX(20px)';
                    parent.style.transition = 'all 0.2s ease';
                    
                    setTimeout(function() {
                        parent.remove();
                    }, 200);
                }
            });
        }
    });
}

/* ===================================================================
   TOOLTIP/POPOVER HELPERS
   Basic tooltip functionality (can be enhanced later)
   =================================================================== */

window.ModernTooltip = {
    /**
     * Show a simple tooltip
     * @param {HTMLElement} element - The element to show tooltip for
     * @param {string} text - The tooltip text
     */
    show: function(element, text) {
        const tooltip = document.createElement('div');
        tooltip.className = 'modern-tooltip';
        tooltip.textContent = text;
        tooltip.style.cssText = `
            position: absolute;
            background: rgba(0, 0, 0, 0.9);
            color: white;
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 12px;
            z-index: 9999;
            pointer-events: none;
            white-space: nowrap;
        `;
        
        document.body.appendChild(tooltip);
        
        const rect = element.getBoundingClientRect();
        tooltip.style.top = (rect.top - tooltip.offsetHeight - 5) + 'px';
        tooltip.style.left = (rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2)) + 'px';
        
        return tooltip;
    },
    
    /**
     * Hide a tooltip
     * @param {HTMLElement} tooltip - The tooltip element to hide
     */
    hide: function(tooltip) {
        if (tooltip && tooltip.parentNode) {
            tooltip.remove();
        }
    }
};

/* ===================================================================
   UTILITY FUNCTIONS
   =================================================================== */

/**
 * Smooth scroll to an element
 * @param {string} elementId - The ID of the element to scroll to
 */
window.smoothScrollTo = function(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }
};

/**
 * Copy text to clipboard
 * @param {string} text - The text to copy
 * @returns {Promise<boolean>} - Success status
 */
window.copyToClipboard = async function(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy text:', err);
        return false;
    }
};

/**
 * Show a toast notification
 * @param {string} message - The message to display
 * @param {string} type - The type (info, success, warning, danger)
 * @param {number} duration - Duration in milliseconds (default 3000)
 */
window.showToast = function(message, type = 'info', duration = 3000) {
    const toast = document.createElement('div');
    toast.className = `modern-notification modern-notification-${type}`;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        animation: slideInRight 0.3s ease;
    `;
    toast.textContent = message;
    
    document.body.appendChild(toast);
    
    setTimeout(function() {
        toast.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(function() {
            toast.remove();
        }, 300);
    }, duration);
};

// Add CSS animations for toast
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

/* ===================================================================
   SCROLL TO TOP BUTTON
   Modern floating button that appears when user scrolls down
   =================================================================== */

let scrollToTopInitialized = false;

function initializeScrollToTop() {
    if (scrollToTopInitialized) {
        return;
    }
    
    const scrollBtn = document.getElementById('btnScrollToTop');
    
    if (!scrollBtn) {
        setTimeout(initializeScrollToTop, 500);
        return;
    }
    
    scrollToTopInitialized = true;
}

/* ===================================================================
   BLAZORED MODAL STYLING FORCE
   Forces the modal header to use nav color (#004494) with white text
   =================================================================== */

function forceBlazoredModalStyling() {
    function applyModalFixes() {
        // Remove padding and border from blazored-modal to let header extend to edges
        const modals = document.querySelectorAll('.blazored-modal');
        modals.forEach(modal => {
            modal.style.padding = '0';
            modal.style.border = 'none';
        });
        
        // Force header to use nav color - using setProperty with !important to override CSS isolation
        const headers = document.querySelectorAll('.bm-header');
        headers.forEach(header => {
            
            // Completely clear all background properties
            header.style.removeProperty('background');
            header.style.removeProperty('background-color');
            header.style.removeProperty('background-image');
            header.style.removeProperty('background-size');
            header.style.removeProperty('background-position');
            header.style.removeProperty('background-repeat');
            
            // Apply same gradient as sidebar for visual consistency
            header.style.setProperty('background', 'linear-gradient(180deg, #2d3748 0%, #1a202c 100%)', 'important');
            header.style.setProperty('color', '#ffffff', 'important');
        });
    }
    
    // Apply immediately
    applyModalFixes();
    
    // Apply repeatedly to catch any late-loading modals
    setTimeout(applyModalFixes, 100);
    setTimeout(applyModalFixes, 250);
    setTimeout(applyModalFixes, 500);
    setTimeout(applyModalFixes, 1000);
    
    // Watch for new modals
    const observer = new MutationObserver(function(mutations) {
        let hasNewModal = false;
        mutations.forEach(function(mutation) {
            mutation.addedNodes.forEach(node => {
                if (node.nodeType === 1 && (node.classList?.contains('blazored-modal') || node.querySelector?.('.blazored-modal'))) {
                    hasNewModal = true;
                }
            });
        });
        
        if (hasNewModal) {
            setTimeout(applyModalFixes, 10);
            setTimeout(applyModalFixes, 100);
            setTimeout(applyModalFixes, 250);
        }
    });
    
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
}

/* ===================================================================
   SCROLL TO ELEMENT
   Smooth scroll to a specific element by ID
   =================================================================== */

function scrollToElement(elementId) {
    console.log('Scrolling to element:', elementId);
    
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ 
            behavior: 'smooth', 
            block: 'start',
            inline: 'nearest'
        });
    } else {
        console.warn('Element not found:', elementId);
    }
}

// Make scrollToElement globally available for Blazor interop
window.scrollToElement = scrollToElement;

