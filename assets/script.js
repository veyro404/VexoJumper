  (() => {
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Reveal on scroll
    const items = document.querySelectorAll('.reveal');
    if (!reduce && 'IntersectionObserver' in window) {
      const io = new IntersectionObserver((entries) => {
        entries.forEach((e) => {
          if (e.isIntersecting) {
            e.target.classList.add('visible');
            io.unobserve(e.target);
          }
        });
      }, { threshold: 0.12 });
      items.forEach((el) => io.observe(el));
    } else {
      items.forEach((el) => el.classList.add('visible'));
    }

    // Back to top
    const top = document.getElementById('backtop');
    const update = () => top.classList.toggle('show', window.scrollY > 550);
    window.addEventListener('scroll', update, { passive: true });
    update();

    // Cursor glow follower
    const glow = document.getElementById('cursorGlow');
    if (!reduce && window.matchMedia('(pointer: fine)').matches) {
      window.addEventListener('pointermove', (e) => {
        glow.style.left = e.clientX + 'px';
        glow.style.top = e.clientY + 'px';
      }, { passive: true });
    } else {
      glow.remove();
    }

    // Smooth anchor scroll
    document.querySelectorAll('a[href^="#"]').forEach((a) => {
      a.addEventListener('click', (e) => {
        const target = document.querySelector(a.getAttribute('href'));
        if (target) {
          e.preventDefault();
          target.scrollIntoView({ behavior: reduce ? 'auto' : 'smooth' });
        }
      });
    });

    // Mobile menu drawer
    const toggle = document.querySelector('.menu-toggle');
    const overlay = document.getElementById('mobileMenuOverlay');
    const closeBtn = document.getElementById('mobileMenuClose');
    const mobileLinks = document.querySelectorAll('.mobile-links a');
    const body = document.body;

    function openMenu() {
      overlay.classList.add('open');
      toggle.classList.add('open');
      toggle.setAttribute('aria-expanded', 'true');
      overlay.setAttribute('aria-hidden', 'false');
      body.classList.add('menu-open');
    }

    function closeMenu() {
      overlay.classList.remove('open');
      toggle.classList.remove('open');
      toggle.setAttribute('aria-expanded', 'false');
      overlay.setAttribute('aria-hidden', 'true');
      body.classList.remove('menu-open');
    }

    if (toggle && overlay) {
      toggle.addEventListener('click', (e) => {
        e.stopPropagation();
        if (overlay.classList.contains('open')) closeMenu();
        else openMenu();
      });
    }

    if (closeBtn) closeBtn.addEventListener('click', closeMenu);

    // Close when clicking outside drawer (on overlay background)
    overlay.addEventListener('click', (e) => {
      if (e.target === overlay) closeMenu();
    });

    // Close when clicking any nav link
    mobileLinks.forEach((link) => {
      link.addEventListener('click', closeMenu);
    });

    // Close on ESC key
    window.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && overlay.classList.contains('open')) closeMenu();
    });
  })();
