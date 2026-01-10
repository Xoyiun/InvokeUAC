**[中文繁體](README_zh_TW.md) | [中文简体](README_zh_CN.md) | [English(US)](README_en_US.md)**
# SideUAC
By adding the `RunAsInvoker` compatibility layer to the registry, SideUAC forces specific files to execute with the caller's privileges, effectively bypassing User Account Control (UAC) elevation prompts.

## Core Features

### 1. Modern List Management
* **Automatic Resource Resolution**: Automatically parses and extracts icons from executable paths, providing intuitive visual identification for quick item recognition.
* **Responsive Selection Logic**: Refactored selection mechanisms ensure smooth operational feedback even with large datasets.
* **High-Performance Architecture**: Built on .NET 10.0 to ensure maximum efficiency when handling low-level system registry operations.

### 2. Advanced Batch Operations
* **Multi-Item Selection**: Supports standard Extended Selection mode via mouse dragging, or using Shift and Ctrl keys to select multiple targets simultaneously.
* **Synchronized Processing**: Capability to perform synchronized deletions or file location retrievals for multiple registry keys.

---

## Functional Button Guide

### 1. Add Entry
* **Purpose**: Manually create new UAC bypass registry entries.
* **Usage**: Click the "Add" button and select an `.exe` file or enter a specific command path in the dialog. Upon confirmation, the program will create corresponding values in the targeted registry path.

### 2. Batch Delete
* **Purpose**: Remove bypass configurations for selected items from the system registry.
* **Usage**: Select one or more items from the list and click "Delete." The program invokes registry APIs to execute the deletion and simultaneously removes them from the UI list.

### 3. Batch View Details
* **Purpose**: View detailed paths and icon info of selected items, and locate physical files.
* **Usage**:
    * **Content Preview**: Hover over an item to display its full path via ToolTip.
    * **File Location**: Select an item and click "Location." The program invokes File Explorer to open the directory and highlight the physical file.

### 4. Language Switch
* **Purpose**: Change the UI display language.
* **Usage**: Click the language toggle button. The program dynamically remaps UI strings from JSON language files in real-time (Runtime), allowing language changes without a restart.

### 5. Refresh
* **Purpose**: Rescan the registry and synchronize the latest data state.
* **Usage**: Click the "Refresh" button. The program re-traverses the target registry paths and reloads data to ensure the UI perfectly matches the actual system state.

---

## Technical Specifications
* **Platform**: Windows x64
* **Target Framework**: .NET 10.0
* **User Interface**: WPF (Windows Presentation Foundation)

## Deployment & Downloads

To ensure the application runs correctly, please choose the version suitable for your environment:

### Self-contained (Independent) - Recommended
* **Target Audience**: General users or systems where the .NET 10 environment status is unknown.
* **Description**: Deployed as a Self-contained package including all necessary .NET 10 components. Larger file size but offers the best compatibility.

### Standard (Framework-dependent)
* **Target Audience**: Users who have manually installed the .NET 10 Runtime.
* **Description**: Very small file size, but depends on an existing .NET environment. It will fail to launch if the environment is missing or mismatched.

## Operational Safety Notice
This tool involves write and delete operations within the `HKEY_CURRENT_USER` registry hive. Before performing "Batch Delete" or "Add Entry," ensure the target operations align with your intentions.

# In case of any discrepancies or ambiguities, the Traditional Chinese version shall prevail
