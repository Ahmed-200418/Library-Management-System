// Shared auth and UI helpers used by login/register and index pages
(function () {
    // Only redirect to index if on login/register pages
    const isAuthPage = location.pathname.endsWith('/login.html') || location.pathname.endsWith('/register.html');
    if (isAuthPage && localStorage.getItem('role')) {
        location.replace('/index.html');
    }

    window.showToast = function(message, type = 'info', duration = 4000) {
        const container = document.getElementById('toastContainer');
        if (!container) return;
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;

        const icons = {
            success: '<svg class="toast-icon" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/></svg>',
            error: '<svg class="toast-icon" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/></svg>',
            info: '<svg class="toast-icon" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"/></svg>'
        };

        toast.innerHTML = `${icons[type] || icons.info}<span class="toast-message">${message}</span><svg class="toast-close" fill="currentColor" viewBox="0 0 20 20" onclick="this.closest('.toast').remove()"><path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"/></svg>`;
        container.appendChild(toast);
        if (duration > 0) setTimeout(() => { toast.classList.add('removing'); setTimeout(() => toast.remove(), 300); }, duration);
    };

    window.login = async function(e) {
        e.preventDefault();
        const btn = e.target.querySelector('button');
        const originalText = btn.innerText;

        btn.disabled = true;
        btn.innerText = 'Signing in...';

        try {
            const res = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({
                    email: document.getElementById('email').value,
                    password: document.getElementById('password').value,
                    rememberMe: document.getElementById('remember') ? document.getElementById('remember').checked : false
                })
            });

            if(res.ok) {
                const data = await res.json();
                localStorage.setItem('role', data.role);
                showToast('Welcome back!', 'success', 2000);
                setTimeout(() => location.replace('/index.html'), 1000);
            } else {
                showToast('Invalid email or password.', 'error');
                btn.disabled = false;
                btn.innerText = originalText;
            }
        } catch(err) {
            showToast('Network error occurred.', 'error');
            btn.disabled = false;
            btn.innerText = originalText;
        }
    };

    window.register = async function(e) {
        e.preventDefault();
        const btn = e.target.querySelector('button[type="submit"]');
        const originalText = btn.innerText;

        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const rememberMe = document.getElementById('rememberMe') ? document.getElementById('rememberMe').checked : false;

        btn.disabled = true;
        btn.innerText = 'Creating account...';

        try {
            const res = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password, rememberMe })
            });

            if (res.ok) {
                showToast('Account created! Redirecting...', 'success', 2000);
                setTimeout(() => location.replace('/index.html'), 1500);
            } else {
                const errorText = await res.text();
                showToast('Failed: ' + errorText, 'error');
                btn.disabled = false;
                btn.innerText = originalText;
            }
        } catch (err) {
            console.error(err);
            showToast('Network error occurred.', 'error');
            btn.disabled = false;
            btn.innerText = originalText;
        }
    };
})();
