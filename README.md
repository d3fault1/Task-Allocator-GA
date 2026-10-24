# Task Allocator

A Windows client for the same task-to-processor search as [HeuristicSearch](https://github.com/d3fault1/HeuristicSearch). This version calls hosted services instead of local consoles.

---

## Overview

- Two web services, `AlgorithmServerOne` and `AlgorithmServerTwo`.
- A form with a list of ready-made configuration files.
- Results show runtime and energy for each schedule.

---

## Tech Stack

| Component | Technology |
| --- | --- |
| **Language** | C# |
| **UI** | Windows Forms, .NET Framework 4.7.2 |
| **Services** | WCF |
| **Search** | GeneticSharp 2.6.0 |

---

## Project Structure

```
Task-Allocator-GA/
├── PT2.sln
├── AlgorithmServerOne/
├── AlgorithmServerTwo/
├── ClientGUI/
├── FileFormat/
└── CommonInterfaces/
```

---

## Prerequisites

- Visual Studio with the .NET desktop workload (.NET Framework 4.7.2)
- The services reachable at the addresses in `ClientGUI/app.config`, or IIS if you host them yourself

---

## Setup

Open `PT2.sln`, restore the NuGet packages, and build.

To host the services yourself, put their addresses in `ClientGUI/app.config` and rebuild the client.

---

## Run

Start `ClientGUI`. Pick a configuration file from the list and run.
