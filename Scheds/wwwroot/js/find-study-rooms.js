document.getElementById('search-button').addEventListener('click', function() {
    const day = document.getElementById('day-select').value;
    const time = document.getElementById('time-select').value;

    // Example AJAX request (assuming you're using a backend to fetch available rooms)
    fetch(`/api/find-rooms?day=${day}&time=${time}`)
        .then(response => response.json())
        .then(data => {
            const resultsDiv = document.getElementById('results');
            resultsDiv.innerHTML = '';

            if (data.rooms.length === 0) {
                resultsDiv.innerHTML = '<p>No available rooms found for the selected day and time.</p>';
            } else {
                const list = document.createElement('ul');
                data.rooms.forEach(room => {
                    const listItem = document.createElement('li');
                    listItem.textContent = room;
                    list.appendChild(listItem);
                });
                resultsDiv.appendChild(list);
            }
        })
        .catch(error => {
            console.error('Error fetching room data:', error);
        });
});
