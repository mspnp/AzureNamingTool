// Drag and Drop Sorting for Configuration Page
// Handles reordering of sortable items with visual feedback

// Register the Configuration page component for callbacks
window.registerConfigurationPage = function (dotNetReference) {
    window.configurationPage = dotNetReference;
};

window.dragDropSort = {
    draggedElement: null,
    placeholder: null,
    originalParent: null,
    originalIndex: null,
    targetContainer: null,

    // Initialize drag-and-drop for a container
    init: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const items = container.querySelectorAll('[draggable="true"]');
        items.forEach(item => {
            this.attachHandlers(item, containerId);
        });
    },

    // Attach event handlers to a draggable item
    attachHandlers: function (item, containerId) {
        item.addEventListener('dragstart', (e) => this.handleDragStart(e, containerId));
        item.addEventListener('dragend', (e) => this.handleDragEnd(e));
        item.addEventListener('dragover', (e) => this.handleDragOver(e));
        item.addEventListener('dragleave', (e) => this.handleDragLeave(e));
        // Note: drop event is handled on dragover items, not on the item itself
    },

    // Handle drag start event
    handleDragStart: function (e, containerId) {
        this.draggedElement = e.currentTarget;
        this.originalParent = this.draggedElement.parentElement;
        this.originalIndex = Array.from(this.originalParent.children).indexOf(this.draggedElement);
        this.targetContainer = document.getElementById(containerId);

        // Add dragging class for visual feedback
        this.draggedElement.classList.add('dragging');
        
        // Set drag data
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', this.draggedElement.innerHTML);

        // Create placeholder
        this.placeholder = document.createElement('tr');
        this.placeholder.className = 'drag-placeholder';
        this.placeholder.innerHTML = `<td colspan="100" style="height: ${this.draggedElement.offsetHeight}px;"></td>`;
    },

    // Handle drag over event
    handleDragOver: function (e) {
        if (e.preventDefault) {
            e.preventDefault();
        }
        e.dataTransfer.dropEffect = 'move';

        const target = e.currentTarget;
        
        // Only allow dropping on draggable items within the same container
        if (target.hasAttribute('draggable') && target !== this.draggedElement) {
            // Check if target is in the same container
            if (this.targetContainer && this.targetContainer.contains(target)) {
                // Determine if we should insert before or after
                const rect = target.getBoundingClientRect();
                const midpoint = rect.top + (rect.height / 2);
                
                if (e.clientY < midpoint) {
                    // Insert before
                    target.parentElement.insertBefore(this.placeholder, target);
                } else {
                    // Insert after
                    target.parentElement.insertBefore(this.placeholder, target.nextSibling);
                }
                
                target.classList.add('drag-over');
            }
        }

        return false;
    },

    // Handle drag leave event
    handleDragLeave: function (e) {
        e.currentTarget.classList.remove('drag-over');
    },

    // Handle drag end event
    handleDragEnd: function (e) {
        const draggedElement = this.draggedElement;
        
        // Remove dragging class
        if (draggedElement) {
            draggedElement.classList.remove('dragging');
        }

        // Insert dragged element at placeholder position if placeholder exists
        if (this.placeholder && this.placeholder.parentElement && draggedElement) {
            this.placeholder.parentElement.insertBefore(draggedElement, this.placeholder);
        }

        // Remove placeholder
        if (this.placeholder && this.placeholder.parentElement) {
            this.placeholder.remove();
        }

        // Remove all drag-over classes
        document.querySelectorAll('.drag-over').forEach(el => {
            el.classList.remove('drag-over');
        });

        if (!draggedElement) {
            return null;
        }

        // Check if the item is still in the valid container
        const currentParent = draggedElement.parentElement;
        const isInValidContainer = this.targetContainer && this.targetContainer.contains(draggedElement);

        if (!isInValidContainer) {
            // Item was dragged outside - return it to original position
            console.log('Item dragged outside container, returning to original position');
            const children = Array.from(this.originalParent.children);
            if (this.originalIndex >= children.length) {
                this.originalParent.appendChild(draggedElement);
            } else {
                this.originalParent.insertBefore(draggedElement, children[this.originalIndex]);
            }
            return null; // No change
        }

        // Calculate new index
        const newIndex = Array.from(currentParent.children).indexOf(draggedElement);

        // Check if position actually changed
        if (currentParent === this.originalParent && newIndex === this.originalIndex) {
            return null; // No change
        }

        // Trigger callback based on container ID
        const containerId = this.targetContainer.id;
        this.triggerCallback(containerId);

        // Get the item ID and new sort order
        const itemId = draggedElement.getAttribute('data-item-id');
        const newSortOrder = newIndex + 1; // Sort order is 1-based

        // Return the reorder data
        return {
            itemId: parseInt(itemId),
            newSortOrder: newSortOrder,
            oldSortOrder: this.originalIndex + 1
        };
    },

    // Get reorder data after drag completes
    getReorderData: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Container not found:', containerId);
            return null;
        }

        const items = container.querySelectorAll('[draggable="true"]');
        const reorderData = [];

        items.forEach((item, index) => {
            const itemId = item.getAttribute('data-item-id');
            if (itemId) {
                reorderData.push({
                    itemId: parseInt(itemId),
                    sortOrder: index + 1
                });
            }
        });

        return reorderData;
    },

    // Trigger appropriate callback based on container ID
    triggerCallback: function (containerId) {
        if (!window.configurationPage) {
            console.error('Configuration page reference not found!');
            return;
        }
        
        // Map container IDs to their C# handler methods
        if (containerId === 'resourceComponentsTableBody') {
            console.log('Calling HandleResourceComponentReorder');
            window.configurationPage.invokeMethodAsync('HandleResourceComponentReorder');
        } else if (containerId.startsWith('customComponent')) {
            // Extract parent component ID from container ID
            const match = containerId.match(/customComponent(\d+)TableBody/);
            if (match) {
                const parentComponentId = parseInt(match[1]);
                window.configurationPage.invokeMethodAsync('HandleCustomComponentReorder', parentComponentId);
            }
        } else if (containerId === 'resourceEnvironmentsTableBody') {
            window.configurationPage.invokeMethodAsync('HandleEnvironmentReorder');
        } else if (containerId === 'resourceFunctionsTableBody') {
            window.configurationPage.invokeMethodAsync('HandleFunctionReorder');
        } else if (containerId === 'resourceOrgsTableBody') {
            window.configurationPage.invokeMethodAsync('HandleOrgReorder');
        } else if (containerId === 'resourceProjAppSvcsTableBody') {
            window.configurationPage.invokeMethodAsync('HandleProjAppSvcReorder');
        } else if (containerId === 'resourceUnitDeptsTableBody') {
            window.configurationPage.invokeMethodAsync('HandleUnitDeptReorder');
        } else {
            console.warn('Unknown container ID:', containerId);
        }
    },

    // Cleanup
    destroy: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const items = container.querySelectorAll('[draggable="true"]');
        items.forEach(item => {
            item.removeEventListener('dragstart', this.handleDragStart);
            item.removeEventListener('dragend', this.handleDragEnd);
            item.removeEventListener('dragover', this.handleDragOver);
            item.removeEventListener('drop', this.handleDrop);
            item.removeEventListener('dragleave', this.handleDragLeave);
        });
    }
};
