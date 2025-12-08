/* ===================================================================
   Azure Naming Tool - Theme Management
   Version: 1.0
   Date: October 17, 2025
   Description: JavaScript helper for light/dark theme management
   =================================================================== */

(function() {
    'use strict';

    // ===================================================================
    // Theme Management Functions
    // ===================================================================

    /**
     * Get the system's preferred color scheme
     * @returns {string} 'dark' or 'light'
     */
    function getSystemTheme() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        return 'light';
    }

    /**
     * Get the current theme from localStorage or system preference
     * @returns {string} 'dark' or 'light'
     */
    window.getTheme = function() {
        try {
            const savedTheme = localStorage.getItem('theme');
            if (savedTheme === 'dark' || savedTheme === 'light') {
                return savedTheme;
            }
            // If no saved theme, use system preference
            return getSystemTheme();
        } catch (e) {
            console.warn('LocalStorage not available, using system theme:', e);
            return getSystemTheme();
        }
    };

    /**
     * Set the theme and persist it to localStorage
     * @param {string} theme - 'dark' or 'light'
     */
    window.setTheme = function(theme) {
        if (theme !== 'dark' && theme !== 'light') {
            console.error('Invalid theme:', theme);
            return;
        }

        try {
            // Add transitioning class for smooth theme change
            document.documentElement.classList.add('theme-transitioning');
            
            // Set the data-theme attribute
            document.documentElement.setAttribute('data-theme', theme);
            
            // Save to localStorage
            localStorage.setItem('theme', theme);
            
            // Remove transitioning class after transition completes
            setTimeout(() => {
                document.documentElement.classList.remove('theme-transitioning');
            }, 300);

            // Dispatch custom event for theme change
            window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
        } catch (e) {
            console.error('Error setting theme:', e);
        }
    };

    /**
     * Toggle between light and dark themes
     * @returns {string} The new theme
     */
    window.toggleTheme = function() {
        const currentTheme = window.getTheme();
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        window.setTheme(newTheme);
        return newTheme;
    };

    /**
     * Initialize theme on page load
     */
    function initTheme() {
        const theme = window.getTheme();
        document.documentElement.setAttribute('data-theme', theme);
    }

    /**
     * Watch for system theme changes
     */
    function watchSystemTheme() {
        if (window.matchMedia) {
            const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
            
            // Modern browsers
            if (darkModeQuery.addEventListener) {
                darkModeQuery.addEventListener('change', (e) => {
                    // Only apply system theme change if user hasn't explicitly set a theme
                    try {
                        const savedTheme = localStorage.getItem('theme');
                        if (!savedTheme) {
                            const newTheme = e.matches ? 'dark' : 'light';
                            window.setTheme(newTheme);
                            console.log('System theme changed to:', newTheme);
                        }
                    } catch (err) {
                        console.warn('Error watching system theme:', err);
                    }
                });
            }
            // Older browsers
            else if (darkModeQuery.addListener) {
                darkModeQuery.addListener((e) => {
                    try {
                        const savedTheme = localStorage.getItem('theme');
                        if (!savedTheme) {
                            const newTheme = e.matches ? 'dark' : 'light';
                            window.setTheme(newTheme);
                        }
                    } catch (err) {
                        console.warn('Error watching system theme:', err);
                    }
                });
            }
        }
    }

    // ===================================================================
    // Initialize on page load
    // ===================================================================

    // Initialize theme immediately (before DOM loads) to prevent flash
    initTheme();

    // Watch for system theme changes
    watchSystemTheme();

    // Re-initialize theme when DOM is fully loaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTheme);
    } else {
        // DOMContentLoaded already fired
        initTheme();
    }

    // ===================================================================
    // Blazor Interop Helpers
    // ===================================================================

    /**
     * Get theme for Blazor component
     * Used by ThemeToggle.razor component
     */
    window.blazorTheme = {
        getTheme: function() {
            return window.getTheme();
        },
        setTheme: function(theme) {
            window.setTheme(theme);
        },
        toggleTheme: function() {
            return window.toggleTheme();
        }
    };
})();
