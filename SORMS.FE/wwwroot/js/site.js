// ===== SIDEBAR FUNCTIONALITY =====
$(document).ready(function () {
    // Sidebar toggle functionality
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('collapsed');
        $('#content').toggleClass('expanded');
    });

    // Mobile sidebar toggle
    $('.navbar-toggler').on('click', function () {
        $('#sidebar').toggleClass('show');
    });

    // Close sidebar when clicking outside on mobile
    $(document).on('click', function (e) {
        if ($(window).width() <= 576) {
            if (!$(e.target).closest('#sidebar, .navbar-toggler').length) {
                $('#sidebar').removeClass('show');
            }
        }
    });

    // Active menu item highlighting
    $('.sidebar .nav-link').on('click', function () {
        $('.sidebar .nav-link').removeClass('active');
        $(this).addClass('active');
    });

    // Auto-collapse sidebar on mobile
    if ($(window).width() <= 768) {
        $('#sidebar').addClass('collapsed');
        $('#content').addClass('expanded');
    }

    // Handle window resize
    $(window).on('resize', function () {
        if ($(window).width() <= 768) {
            $('#sidebar').addClass('collapsed');
            $('#content').addClass('expanded');
        } else {
            $('#sidebar').removeClass('collapsed');
            $('#content').removeClass('expanded');
        }
    });
});

// ===== FORM VALIDATION AND ENHANCEMENTS =====
$(document).ready(function () {
    // Add loading state to forms
    $('form').on('submit', function () {
        const submitBtn = $(this).find('button[type="submit"]');
        const originalText = submitBtn.text();
        
        submitBtn.prop('disabled', true)
                .html('<span class="spinner-modern me-2"></span>Đang xử lý...');
        
        // Re-enable button after 3 seconds (fallback)
        setTimeout(function () {
            submitBtn.prop('disabled', false).text(originalText);
        }, 3000);
    });

    // Enhanced form controls
    $('.form-control').on('focus', function () {
        $(this).parent().addClass('focused');
    }).on('blur', function () {
        if (!$(this).val()) {
            $(this).parent().removeClass('focused');
        }
    });

    // Auto-format currency inputs
    $('input[type="number"][step="0.01"]').on('input', function () {
        let value = $(this).val();
        if (value && !isNaN(value)) {
            $(this).val(parseFloat(value).toFixed(2));
        }
    });
});

// ===== NOTIFICATION SYSTEM =====
function showNotification(message, type = 'info', duration = 5000) {
    const notification = $(`
        <div class="alert alert-modern alert-${type} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="fas fa-${getNotificationIcon(type)} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    
    $('body').append(notification);
    
    // Auto remove after duration
    setTimeout(function () {
        notification.alert('close');
    }, duration);
}

function getNotificationIcon(type) {
    const icons = {
        'success': 'check-circle',
        'danger': 'exclamation-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// ===== DATA TABLE ENHANCEMENTS =====
$(document).ready(function () {
    // Add search functionality to tables
    $('.table-modern').each(function () {
        const table = $(this);
        const tableId = 'table-' + Math.random().toString(36).substr(2, 9);
        table.attr('id', tableId);
        
        // Add search input
        const searchInput = $(`
            <div class="table-search mb-3">
                <div class="input-group">
                    <span class="input-group-text"><i class="fas fa-search"></i></span>
                    <input type="text" class="form-control" placeholder="Tìm kiếm..." 
                           data-table-target="${tableId}">
                </div>
            </div>
        `);
        
        table.before(searchInput);
        
        // Implement search functionality
        searchInput.find('input').on('keyup', function () {
            const searchTerm = $(this).val().toLowerCase();
            table.find('tbody tr').each(function () {
                const rowText = $(this).text().toLowerCase();
                $(this).toggle(rowText.includes(searchTerm));
            });
        });
    });

    // Add row hover effects
    $('.table-modern tbody tr').hover(
        function () {
            $(this).addClass('table-hover-effect');
        },
        function () {
            $(this).removeClass('table-hover-effect');
        }
    );
});

// ===== CHART INITIALIZATION =====
function initializeCharts() {
    // Dashboard statistics charts
    if (typeof Chart !== 'undefined') {
        // Revenue Chart
        const revenueCtx = document.getElementById('revenueChart');
        if (revenueCtx) {
            new Chart(revenueCtx, {
                type: 'line',
                data: {
                    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
                    datasets: [{
                        label: 'Doanh thu',
                        data: [12000000, 15000000, 18000000, 16000000, 20000000, 22000000],
                        borderColor: '#4f46e5',
                        backgroundColor: 'rgba(79, 70, 229, 0.1)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function(value) {
                                    return value.toLocaleString('vi-VN') + ' VNĐ';
                                }
                            }
                        }
                    }
                }
            });
        }

        // Occupancy Chart
        const occupancyCtx = document.getElementById('occupancyChart');
        if (occupancyCtx) {
            new Chart(occupancyCtx, {
                type: 'doughnut',
                data: {
                    labels: ['Đã thuê', 'Trống'],
                    datasets: [{
                        data: [75, 25],
                        backgroundColor: ['#10b981', '#e2e8f0'],
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        }
    }
}

// ===== UTILITY FUNCTIONS =====
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function formatDate(date) {
    return new Intl.DateTimeFormat('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    }).format(new Date(date));
}

function formatDateTime(date) {
    return new Intl.DateTimeFormat('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    }).format(new Date(date));
}

// ===== MODAL ENHANCEMENTS =====
$(document).ready(function () {
    // Auto-focus first input in modals
    $('.modal').on('shown.bs.modal', function () {
        $(this).find('input, textarea, select').first().focus();
    });

    // Confirm dialogs for delete actions
    $('.btn-delete').on('click', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        const itemName = $(this).data('item-name') || 'mục này';
        
        if (confirm(`Bạn có chắc chắn muốn xóa ${itemName}?`)) {
            window.location.href = url;
        }
    });
});

// ===== LAZY LOADING FOR IMAGES =====
$(document).ready(function () {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }
});

// ===== AUTO-SAVE FORMS =====
$(document).ready(function () {
    // Auto-save form data to localStorage
    $('form[data-autosave]').each(function () {
        const form = $(this);
        const formId = form.attr('id') || 'form-' + Math.random().toString(36).substr(2, 9);
        form.attr('id', formId);
        
        // Load saved data
        const savedData = localStorage.getItem('form-' + formId);
        if (savedData) {
            const data = JSON.parse(savedData);
            Object.keys(data).forEach(key => {
                form.find(`[name="${key}"]`).val(data[key]);
            });
        }
        
        // Save data on input change
        form.find('input, textarea, select').on('input change', function () {
            const formData = form.serializeArray();
            const data = {};
            formData.forEach(field => {
                data[field.name] = field.value;
            });
            localStorage.setItem('form-' + formId, JSON.stringify(data));
        });
        
        // Clear saved data on successful submit
        form.on('submit', function () {
            localStorage.removeItem('form-' + formId);
        });
    });
});

// ===== INITIALIZATION =====
$(document).ready(function () {
    // Initialize all components
    initializeCharts();
    
    // Add fade-in animation to main content
    $('.main-content').addClass('fade-in');
    
    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Initialize popovers
    $('[data-bs-toggle="popover"]').popover();
});

// ===== EXPORT FUNCTIONS FOR GLOBAL USE =====
window.SORMS = {
    showNotification: showNotification,
    formatCurrency: formatCurrency,
    formatDate: formatDate,
    formatDateTime: formatDateTime
};