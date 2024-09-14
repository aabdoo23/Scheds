// Simple scroll animation effect
window.addEventListener('scroll', function () {
    const features = document.querySelectorAll('.feature');
    const scrollPosition = window.scrollY + window.innerHeight;

    features.forEach(feature => {
        if (scrollPosition > feature.offsetTop + feature.offsetHeight / 2) {
            feature.style.opacity = '1';
            feature.style.transform = 'translateY(0)';
        }
    });
});
