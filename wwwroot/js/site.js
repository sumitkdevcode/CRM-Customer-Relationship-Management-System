// CRM System - JavaScript functionality

document.addEventListener('DOMContentLoaded', function () {
    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            alert.style.transition = 'opacity 0.5s ease';
            alert.style.opacity = '0';
            setTimeout(function () {
                alert.remove();
            }, 500);
        }, 5000);
    });

    // Sidebar mobile toggle
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.querySelector('.sidebar');
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('active');
        });
    }

    // Confirm delete actions
    const deleteButtons = document.querySelectorAll('.btn-danger[type="submit"]');
    deleteButtons.forEach(function (btn) {
        if (!btn.hasAttribute('onclick')) {
            btn.addEventListener('click', function (e) {
                if (!confirm('Are you sure you want to delete this item?')) {
                    e.preventDefault();
                }
            });
        }
    });

    // Form validation styling
    const forms = document.querySelectorAll('form');
    forms.forEach(function (form) {
        form.addEventListener('submit', function () {
            const inputs = form.querySelectorAll('.form-control');
            inputs.forEach(function (input) {
                if (!input.checkValidity()) {
                    input.classList.add('is-invalid');
                } else {
                    input.classList.remove('is-invalid');
                }
            });
        });
    });

    // Search functionality
    const searchInput = document.querySelector('.header-search input');
    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                // Implement global search
                console.log('Search:', this.value);
            }
        });
    }

    // Initialize tooltips (if using Bootstrap)
    const tooltips = document.querySelectorAll('[title]');
    tooltips.forEach(function (tooltip) {
        tooltip.setAttribute('data-toggle', 'tooltip');
    });
});

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

// Format date
function formatDate(date) {
    return new Intl.DateTimeFormat('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    }).format(new Date(date));
}
