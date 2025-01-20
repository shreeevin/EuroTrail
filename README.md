# Financial Management App

## Overview

The **Financial Management App** is a comprehensive tool designed to help individuals track and manage their financial activities. This app allows users to keep track of their  **budget** ,  **expenses** ,  **income** , and  **debts** . The primary goal of this app is to offer a simple yet powerful solution for managing personal finances, ensuring better budgeting, financial analysis, and overall financial health.

## Features

### 1. **Track Budget, Expense, Income, and Debt**

* **Budget:** Set and manage monthly or yearly budgets.
* **Expense:** Track your daily expenses, categorize them, and set spending limits.
* **Income:** Log your income sources and keep track of monthly earnings.
* **Debt:** Monitor your debts, including balances and repayment schedules.

### 2. **CRUD Operations (Create, Read, Update, Delete)**

* **Add:** Add new entries for budget, expenses, income, and debts.
* **Edit:** Modify existing entries for better management.
* **Delete:** Remove entries when they are no longer needed or relevant.

### 3. **Search Functionality**

* Easily search through budgets, expenses, incomes, or debts by date, category, or amount to get quick insights and make decisions.

### 4. **Financial Analysis and Reports**

* **Budget Analysis:** Visualize how well you're sticking to your budget.
* **Expense Trends:** View trends over time to understand spending habits.
* **Debt Payoff Tracker:** Analyze the remaining debt and track progress toward repayment.

## Prerequisites

Before you start, ensure you have the following installed:

1. **.NET  or later** (for .NET MAUI development):
   * Download from [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download/dotnet).
2. **Visual Studio 2022 or later** with **MAUI Workload** installed:
   * Follow the official installation guide from Microsoft: [Install .NET MAUI with Visual Studio](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation).

## Dependencies

1. **.NET MAUI** : Used for cross-platform mobile and desktop development.
2. **.NET MAUI Windows** : For building applications for Windows.
3. **.NET MAUI Mac Catalyst** : For building applications for macOS.
4. **Microsoft.Data.Sqlite** : For database management.
5. **Newtonsoft.Json** : For data Serialization & Deserialization.
6. **BCrypt.Net-Next** (optional): For Encryption & Decryption .
7. **Community Toolkit for .NET MAUI** (optional): For UI and functionality enhancements.

### Installation

1. Clone the repository to your local machine:

```bash
git clone https://github.com/shreeevin/EuroTrail.git
```

2. Open the project in  **Visual Studio** .
3. Navigate to the project directory (`terminal` `cli`):

```bash
cd EuroTrail
```

4. Restore the NuGet packages:

   - Right-click on the solution and click Restore NuGet Packages or use the following command in the terminal:

   ```bash
   dotnet restore
   ```

   - Install the necessary dependencies:

   ```bash
   npm install
   ```

   - Run the tailwind :

   ```bash
   npm run build-and-watch
   ```
5. Build the project for your desired platform (Android, iOS, Windows, Mac Catalyst):

   - Windows:

   ```bash
   dotnet build -t:win10
   ```

   - Mac Catalyst:

   ```bash
   dotnet build -t:macCatalyst
   ```
6. Run the application for the desired platform:

   - Windows:

   ```bash
   dotnet maui windows
   ```

   - Mac Catalyst:

   ```bash
   dotnet maui macCatalyst
   ```
7. If you are targeting other platforms (Android or iOS), ensure you have the respective emulators or devices configured and use the appropriate dotnet `maui` commands for those platforms.

## Usage

1. **Dashboard:** Upon logging in, users can view an overview of their financial status, including total income, expenses, budget balance, and debts.
2. **Add Entries:** Use the "Add" button to enter new records for income, expenses, budget goals, or debts.
3. **Analyze Data:** View graphical reports and breakdowns to get insights into your spending, income, and debt status.
4. **Search:** Use the search functionality to find specific records or categories of transactions.

## Contributing

We welcome contributions to improve the Financial Management App! If you'd like to contribute, please follow these steps:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-name`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature-name`).
5. Create a new Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
