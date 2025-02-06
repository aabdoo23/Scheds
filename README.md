![Scheds](https://socialify.git.ci/aabdoo23/Scheds/image?language=1&name=1&owner=1&pattern=Solid&theme=Dark)

# Scheds - NU Schedule Generator

Scheds is a web application designed to help students at Nile University (NU) in Egypt create personalized class schedules. It allows users to search for courses, customize their schedule preferences, and generate multiple schedule options based on their criteria.

## Scheds is live now!!! 🚀🚀
Test it here: **https://scheds.runasp.net** ✨

## Features

*   **Course Search:**
    *   Search for courses using course codes or names.
    *   Live data fetching from NU's Self-Service system to ensure up-to-date information.
    *   Autocomplete suggestions as you type.
*   **Cart System:**
    *   Add courses to a cart for schedule generation.
    *   Remove courses from the cart.
    *   Persistent cart using browser cookies.
*   **Schedule Customization:**
    *   Set minimum slots per day.
    *   Set maximum gap between classes.
    *   Set maximum number of generated schedules.
    *   Select specific days of the week or a maximum number of days on campus.
    *   Set the earliest start time and latest end time for classes.
    *   Option for Engineering students to handle labs and tutorials.
*   **Schedule Generation:**
    *   Generates multiple timetable options based on user preferences.
    *   Filters schedules based on specified constraints.
    *   Handles multiple sections, labs, and tutorials.
    *   Option to use live data or cached data.
*   **Study Room Finder:**
    *   Find empty study rooms at specific times and days.
*   **User Interface:**
    *   Responsive design for desktop and mobile.
    *   Dark and Light mode.
    *   Clear and intuitive layout for easy navigation.
*   **Backend API:**
    *   RESTful API for handling data requests and schedule generation.

## Technologies Used

*   **Frontend:**
    *   HTML
    *   CSS (Custom styles with CSS variables)
    *   JavaScript
    *   jQuery
    *   Bootstrap (for basic layout and components)
*   **Backend:**
    *   C# (.NET 8)
    *   ASP.NET Core MVC
    *   Entity Framework Core (for database interactions)
*   **Database:**
    *   SQL Server (Can be configured to use other databases)
*   **Libraries:**
    *   jQuery Validation
    *   Newtonsoft.Json
    *   Bootstrap


## Getting Started

### Prerequisites

*   .NET SDK 8.0 or higher
*   SQL Server or another compatible database
*   A web browser

### Installation

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/aabdoo23/Scheds.git
    cd Scheds
    ```
2.  **Configure the database:**
    *   Update the connection string in `appsettings.json` and `appsettings.Development.json` with your SQL Server connection details.
    *   Apply migrations to create the database:
        ```bash
        dotnet ef database update
        ```
3.  **Build and Run:**

    ```bash
    dotnet build
    dotnet run
    ```
4.  **Open the application in your browser:** Navigate to `https://localhost:7250` or `http://localhost:5254`.

## Usage

1.  **Course Search:**
    *   Use the search bar on the "Generate Schedules" page to find courses.
    *   Click the "Search Live" button to fetch updated data from the Self-Service system.
    *   Click on a course result to add it to your cart.
2.  **Cart Management:**
    *   View selected courses in the cart section.
    *   Click on a course in the cart to remove it.
    *   Use the "Clear Cart" button to remove all courses.
3.  **Schedule Customization:**
    *   Adjust the sliders for minimum slots per day, maximum gap period, and maximum number of schedules.
    *   Choose between selecting a maximum number of days or specific days of the week.
    *   Select the start and end times for your schedule.
    *   Check the "Engineering Student" checkbox if applicable.
4.  **Schedule Generation:**
    *   Click the "Generate Schedules" button to generate timetable options.
    *   View the generated schedules below the customization options.
5.  **Study Room Finder:**
    *   Navigate to the "Find Study Rooms" page.
    *   Select a day and time.
    *   Click "Search" to find available rooms.
6.  **Theme Toggle:**
    *   Use the toggle button in the top right of the navigation bar to switch between dark and light modes.

## API Endpoints

*   `GET /api/card`: Get all card items.
*   `GET /api/card/byId/{id}`: Get a card item by ID.
*   `GET /api/card/{courseCode}`: Get card items by course code.
*   `POST /api/cart/add`: Add a course to the cart.
*   `POST /api/cart/remove`: Remove a course from the cart.
*   `GET /api/cart/getCartItems`: Get all cart items.
*   `POST /api/cart/generate`: Save the generate request to cookies.
*   `GET /api/cart/getGenerateRequest`: Get the generate request from cookies.
*   `POST /api/cart/clear`: Clear the cart and generate request cookies.
*   `GET /api/coursebase/getAllCourses`: Get all course bases.
*   `GET /api/coursebase/search/{query}`: Search for course bases.
*   `GET /api/coursebase/getCourseBaseByCourseCode`: Get a course base by course code.
*   `GET /api/coursebase/getCourseBaseByName`: Get a course base by course name.
*   `POST /api/coursebase/addCourseBase`: Add a new course base.
*   `GET /api/courseSchedule`: Get course schedules by card ID.
*   `POST /api/courseSchedule`: Add a new course schedule.
*   `GET /api/instructor/getAllInstructors`: Get all instructors.
*   `GET /api/room?dayOfWeek={dayOfWeek}&time={time}`: Get empty rooms for a specific day and time.
*   `POST /api/generate`: Generate timetables based on the request.

## Contributing

Contributions are welcome! If you have any ideas, bug reports, or feature requests, please feel free to submit a pull request or open an issue.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.
