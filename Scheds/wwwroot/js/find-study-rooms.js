document.getElementById('search-button').addEventListener('click', function () {
    const day = document.getElementById('day-select').value;
    const time = document.getElementById('time-select').value;

    // Construct the URL with query parameters, ensuring special characters are encoded
    const url = `/api/room?dayOfWeek=${encodeURIComponent(day)}&time=${encodeURIComponent(time)}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            console.log(data);
            const resultsDiv = document.getElementById('results');
            resultsDiv.innerHTML = '';

            if (data.length === 0) {
                resultsDiv.innerHTML = '<p>No available rooms found for the selected day and time.</p>';
            } else {
                const list = document.createElement('ul');
                data.forEach(room => {
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
