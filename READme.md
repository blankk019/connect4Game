# Connect 4 Game (Windows Forms - C#)

A simple **Connect 4** game built using **Windows Forms in C#**.  
The project supports two play modes:
-  Player vs Player (PvP)
-  Player vs Computer (PvE) *(basic random-move logic)*

This project was created to practice **C# fundamentals**, **Windows Forms UI**, and **basic game logic** implementation.

---

## Features

- 7x6 Connect 4 grid (standard size)
- Player vs Player mode (local two-player)
- Player vs Computer mode (simple random bot)
- Restart button to reset the board anytime
- Visual disc rendering using `OnPaint`
- Win detection for:
  - Horizontal lines  
  - Vertical lines  
  - Diagonal lines (both directions)
- Beginner-friendly, no advanced LINQ or complex OOP patterns used

---

## How the Game Works

- The board is drawn using the **OnPaint()** method.
- Each cell is represented by a colored circle (red or yellow).
- Players take turns clicking a column to drop their disc.
- The game checks for 4 connected discs in:
  - The same row  
  - The same column  
  - Diagonals
- A message box appears when a player wins or if the board is full (draw).

---

## Project Structure

Connect4/
│
├── Form1.cs # Main menu (select mode)
├── PvPForm.cs # Player vs Player logic and UI
├── PvEForm.cs # Player vs Computer logic and UI
├── Program.cs # Application entry point
└── README.md # Project documentation



---

## Technologies Used

- **C# (.NET Framework / WinForms)**
- **Visual Studio / Visual Studio Code**
- Basic **OOP principles**
- **Dictionary** and **List** data structures for move tracking

---

## How to Run

1. Clone or download this repository.
2. Open the `.sln` file using **Visual Studio**.
3. Build and run the project.
4. Choose between **Player vs Player** or **Player vs Bot** mode from the main form.
5. Enjoy the game!

---

## Learning Goals

This project was created to strengthen understanding of:
- Windows Forms controls and event handling
- Custom drawing using `Graphics`
- Game state management with data structures
- Refactoring logic for clarity and beginner-friendliness

---
## Author

Developed by **Eng. Ahmed Mamdouh** 



