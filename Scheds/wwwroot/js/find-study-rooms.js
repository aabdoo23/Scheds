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
                data.forEach(room => {
                    // Create the room card div
                    const roomCard = document.createElement('div');
                    roomCard.classList.add('room-card');
                    
                    // Create an h3 element for the room number
                    const roomTitle = document.createElement('h3');
                    roomTitle.textContent = `Room: ${room}`;
                    
                    // Append the room title to the room card
                    roomCard.appendChild(roomTitle);
                    
                    // Append the room card to the results div
                    resultsDiv.appendChild(roomCard);
                });
            }
        })
        .catch(error => {
            console.error('Error fetching room data:', error);
        });
});
