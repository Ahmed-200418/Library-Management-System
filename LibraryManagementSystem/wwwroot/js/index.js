// Index page specific JS — relies on `showToast` from auth.js
(function () {
    let allBooks = [];
    const role = localStorage.getItem('role');

    if(role === 'Admin') {
        const addBtn = document.getElementById('addBtn');
        if(addBtn) addBtn.classList.remove('hidden');
    }

    function openModal(title, book = null) {
        const modalTitle = document.getElementById('modalTitle');
        const form = document.getElementById('bookForm');
        const imageInput = document.getElementById('imageInput');
        if (modalTitle) modalTitle.innerText = title;
        if (form) form.reset();
        if (book) {
            document.getElementById('bookId').value = book.id;
            form.title.value = book.title;
            form.author.value = book.author;
            form.description.value = book.description;
            imageInput.dataset.oldImagePath = book.imagePath || '';
        } else {
            document.getElementById('bookId').value = '';
            if (imageInput) imageInput.dataset.oldImagePath = '';
        }
        document.getElementById('bookModal').classList.remove('hidden');
    }
    window.openModal = openModal;
    window.closeModal = function() { document.getElementById('bookModal').classList.add('hidden'); };

    window.openViewModal = function(book) {
        const modal = document.getElementById('viewModal');
        document.getElementById('viewTitle').innerText = book.title;
        document.getElementById('viewAuthor').innerText = book.author;
        document.getElementById('viewDesc').innerText = book.description || 'No description available for this book.';
        const img = document.getElementById('viewImage');
        img.src = book.imagePath || 'https://placehold.co/400x600/f3f4f6/a1a1aa?text=No+Cover';
        const statusBadge = document.getElementById('viewStatus');
        if(book.isBorrowed) {
            statusBadge.innerText = 'Borrowed';
            statusBadge.className = 'px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide bg-rose-100 text-rose-700';
        } else {
            statusBadge.innerText = 'Available';
            statusBadge.className = 'px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide bg-emerald-100 text-emerald-700';
        }
        modal.classList.remove('hidden');
    };
    window.closeViewModal = function() { document.getElementById('viewModal').classList.add('hidden'); };

    async function saveBook(event) {
        event.preventDefault();
        const form = event.target;
        const id = document.getElementById('bookId').value;
        const imageInput = document.getElementById('imageInput');
        let url, method, body, headers = {};

        if (id) {
            method = 'POST';
            url = `/api/books/${id}/update-with-image`;
            const formData = new FormData();
            formData.append('id', id);
            formData.append('title', form.title.value);
            formData.append('author', form.author.value);
            formData.append('description', form.description.value);
            if (imageInput.files.length > 0 && imageInput.dataset.oldImagePath) {
                formData.append('oldImagePath', imageInput.dataset.oldImagePath);
                formData.append('image', imageInput.files[0]);
            } else if (imageInput.files.length > 0) {
                formData.append('image', imageInput.files[0]);
            }
            body = formData;
        } else {
            method = 'POST';
            url = '/api/books';
            body = new FormData(form);
        }

        try {
            const res = await fetch(url, { method: method, body: body, headers: headers });
            if (res.ok) { closeModal(); loadBooks(); showToast('Successfully saved!', 'success'); }
            else { showToast('Operation failed.', 'error'); }
        } catch(e) { showToast('Network error.', 'error'); }
    }
    const bookForm = document.getElementById('bookForm');
    if (bookForm) bookForm.addEventListener('submit', saveBook);

    async function loadBooks() {
        try {
            const res = await fetch('/api/books');
            if(res.status === 401) { window.location.href = '/login.html'; return; }
            allBooks = await res.json();
            renderBooks(allBooks);
        } catch (err) {
            document.getElementById('grid').innerHTML = '<p class="text-red-500 text-center col-span-full">Error loading data.</p>';
        }
    }

    function renderBooks(books) {
        const grid = document.getElementById('grid');
        if (!grid) return;
        if (books.length === 0) {
            grid.innerHTML = `<div class="col-span-full text-center py-12"><div class="inline-block p-4 rounded-full bg-gray-100 mb-3"><svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path></svg></div><p class="text-gray-500 font-medium">No books found.</p></div>`;
            return;
        }

        Promise.all(books.map(async (b) => {
            let userBorrowed = false;
            if (b.isBorrowed) {
                try {
                    const checkRes = await fetch(`/api/books/${b.id}/is-borrowed-by-user`);
                    if (checkRes.ok) { const data = await checkRes.json(); userBorrowed = data.hasBorrowed; }
                } catch (err) {}
            }
            return { book: b, userBorrowed: userBorrowed };
        })).then(results => {
            grid.innerHTML = results.map(({ book: b, userBorrowed }) => {
                const safeBook = JSON.stringify(b).replace(/"/g, '&quot;');
                return `
                    <div onclick="openViewModal(${safeBook})" class="cursor-pointer group bg-white rounded-2xl shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 flex flex-col h-full border border-gray-100 overflow-hidden relative">
                        <div class="relative aspect-[3/4] overflow-hidden bg-gray-100">
                            <img src="${b.imagePath || 'https://placehold.co/400x600/f3f4f6/a1a1aa?text=No+Cover'}" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105">
                            <div class="absolute top-3 right-3">
                                <span class="${b.isBorrowed ? 'bg-rose-100 text-rose-700 border-rose-200' : 'bg-emerald-100 text-emerald-700 border-emerald-200'} border text-[10px] px-2.5 py-1 rounded-full font-bold uppercase tracking-wider shadow-sm backdrop-blur-sm bg-opacity-90">
                                    ${b.isBorrowed ? 'Borrowed' : 'Available'}
                                </span>
                            </div>
                        </div>
                        <div class="p-4 flex-grow flex flex-col">
                            <h3 class="font-bold text-lg text-slate-800 leading-snug mb-1 line-clamp-1" title="${b.title}">${b.title}</h3>
                            <p class="text-indigo-500 text-xs font-medium mb-3">${b.author}</p>
                            <p class="text-slate-500 text-xs leading-relaxed line-clamp-3 mb-4 flex-grow">${b.description || 'No description provided.'}</p>
                            <div class="pt-3 border-t border-gray-100 flex items-center justify-between">
                                <div>
                                    ${userBorrowed 
                                        ? `<button onclick="event.stopPropagation(); action(${b.id}, 'return')" class="text-xs font-semibold bg-blue-50 text-blue-600 hover:bg-blue-100 px-3 py-1.5 rounded-md transition">Return</button>`
                                        : (b.isBorrowed 
                                            ? `<span class="text-xs text-gray-400 font-medium italic">Unavailable</span>`
                                            : `<button onclick="event.stopPropagation(); action(${b.id}, 'borrow')" class="text-xs font-semibold bg-emerald-50 text-emerald-600 hover:bg-emerald-100 px-3 py-1.5 rounded-md transition">Borrow</button>` )
                                    }
                                </div>
                                ${role === 'Admin' ? `
                                    <div class="flex items-center gap-1">
                                        <button onclick="event.stopPropagation(); openModal('Edit Book', ${safeBook})" class="p-1.5 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-md transition" title="Edit">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" /></svg>
                                        </button>
                                        <button onclick="event.stopPropagation(); del(${b.id})" class="p-1.5 text-gray-400 hover:text-rose-600 hover:bg-rose-50 rounded-md transition" title="Delete">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                                        </button>
                                    </div>
                                ` : ''}
                            </div>
                        </div>
                    </div>
                `}).join('');
        });
    }

    function filterBooks() {
        const searchInput = document.getElementById('searchInput');
        if(!searchInput) return;
        const query = searchInput.value.trim();
        
        // If search is empty, load all books
        if (query === '') {
            renderBooks(allBooks);
            return;
        }

        // Call the backend search endpoint
        fetch(`/api/books/search/${encodeURIComponent(query)}`)
            .then(res => {
                if (res.status === 401) { window.location.href = '/login.html'; return; }
                return res.json();
            })
            .then(books => renderBooks(books || []))
            .catch(err => {
                document.getElementById('grid').innerHTML = '<p class="text-red-500 text-center col-span-full">Error searching books.</p>';
            });
    }
    window.filterBooks = filterBooks;

    async function action(id, type) {
        try {
            const res = await fetch(`/api/books/${id}/${type}`, { method: 'POST' });
            if(res.ok) { loadBooks(); showToast('Success!', 'success'); } else { showToast('Action failed.', 'error'); }
        } catch(e) { showToast('Network error.', 'error'); }
    }
    window.action = action;

    async function del(id) {
        if (confirm('Are you sure you want to delete this book?')) {
            try {
                const res = await fetch(`/api/books/${id}`, { method: 'DELETE' });
                if(res.ok) { loadBooks(); showToast('Book deleted.', 'success'); } else { showToast('Could not delete.', 'error'); }
            } catch(e) { showToast('Network error.', 'error'); }
        }
    }
    window.del = del;

    async function logout() {
        try { await fetch('/api/auth/logout', { method: 'POST' }); localStorage.removeItem('role'); window.location.href = '/login.html'; } 
        catch(e) { window.location.href = '/login.html'; }
    }
    window.logout = logout;

    // Initial load
    document.addEventListener('DOMContentLoaded', () => {
        loadBooks();
    });
})();
