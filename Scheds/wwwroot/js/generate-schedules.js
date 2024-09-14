document.addEventListener('DOMContentLoaded', function () {
    // Update slider values
    function updateSliderValue(sliderId, valueId) {
        const slider = document.getElementById(sliderId);
        const value = document.getElementById(valueId);
        value.textContent = slider.value;
        slider.addEventListener('input', () => {
            value.textContent = slider.value;
        });
    }

    // Initialize sliders
    updateSliderValue('min-slots', 'min-slots-value');
    updateSliderValue('gap-period', 'gap-period-value');
    updateSliderValue('max-schedules', 'max-schedules-value');
    updateSliderValue('days-on-campus', 'days-on-campus-value');

    // Handle cart interactions
    const courseSearch = document.getElementById('course-search');
    const addToCartButton = document.getElementById('add-to-cart');
    const cartItems = document.getElementById('cart-items');

    addToCartButton.addEventListener('click', () => {
        const course = courseSearch.value.trim();
        if (course) {
            const li = document.createElement('li');
            li.textContent = course;
            cartItems.appendChild(li);
            courseSearch.value = ''; // Clear the input field
        }
    });

    // Handle schedule navigation
    const prevScheduleButton = document.getElementById('prev-schedule');
    const nextScheduleButton = document.getElementById('next-schedule');

    // Dummy schedule index (replace with actual schedule data handling)
    let currentScheduleIndex = 0;

    function updateSchedule() {
        // Logic to update the displayed schedule goes here
        console.log('Displaying schedule index:', currentScheduleIndex);
    }

    prevScheduleButton.addEventListener('click', () => {
        if (currentScheduleIndex > 0) {
            currentScheduleIndex--;
            updateSchedule();
        }
    });

    nextScheduleButton.addEventListener('click', () => {
        // Replace with actual schedule length
        if (currentScheduleIndex < 10) {
            currentScheduleIndex++;
            updateSchedule();
        }
    });

    // Initialize with the first schedule
    updateSchedule();
});