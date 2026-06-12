# Glow - Advanced System Analysis Software

[![GitHub downloads](https://img.shields.io/github/downloads/turkaysoft/glow/total?style=flat&color=1a893c&label=Downloads)](https://github.com/turkaysoft/glow/releases)
[![GitHub stars](https://img.shields.io/github/stars/turkaysoft/glow?style=flat&color=0062cc&label=Stars)](https://github.com/turkaysoft/glow/stargazers)
[![GitHub release](https://img.shields.io/github/v/release/turkaysoft/glow?style=flat&color=5a32a3&label=Latest%20Release)](https://github.com/turkaysoft/glow/releases/latest)
[![Platform](https://img.shields.io/badge/platform-Windows-b31d28?style=flat&label=Platform)](https://github.com/turkaysoft/glow)

**Glow** is an advanced system analysis software developed by **Eray Türkay**. It is designed to uncover technical details that are difficult to access with standard tools or that typically remain completely hidden. From the deepest layers of hardware architecture to the complex structure of software environment variables, Glow analyzes your system with high precision.

**With over 500 advanced features** and comprehensive analysis capabilities, Glow examines system components from multiple angles, making critical data that would normally go unnoticed visible and providing users with broad technical insights.

By presenting all this information in a simple, clear, and organized manner, it enables even users with limited computer knowledge to easily understand their systems and quickly access the data they need through a clean output.

---

### Donate
You can support this project by making a donation to help ensure its sustainability and the development of new features.

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-Donate-0a6628?style=flat&logo=buy-me-a-coffee&logoColor=white)](https://buymeacoffee.com/turkaysoft)

---

## Key Features

* **Privacy First:** Your data stays on your machine; no information is transferred to external servers.
* **Pure Performance:** Developed exclusively in **C# and .NET Framework** with no external libraries or dependencies.
* **Portable:** No installation required. Just download it, extract all files from the ZIP, select the appropriate architecture, and run it.
* **Saving Data:** The ability to save all features offered in the interface as a **Text Document**, **HTML Document** and **Markdown Document**, if desired.
* **System Identity:** Unique device ID generation for hardware verification (Shortcut: `CTRL + G`).
* **Modern UI:** Clean, intuitive interface compatible with Windows 11 design language, featuring Light, Dark, and System themes.
* **Multilingual:** It supports 15 different languages, primarily English. You can access the supported languages here: [Supported Languages](https://github.com/turkaysoft/glow/discussions/20)
* **Built-in Update Mechanism:** It features a built-in smart update mechanism developed specifically by **Türkaysoft**.

---

## Interface Preview

<img width="1010" height="633" alt="Glow UI" src="https://github.com/user-attachments/assets/390d9bbe-6e04-4ca1-a4bd-3548a12cda49" />

---

## Technical Insights & Data Depth

Glow queries deep system layers to provide professional-grade data across specialized modules, showing you what others can't:

### Core Hardware Components
* **Operating System:** Detailed Windows build numbers, installation timestamp, current "Up Time," and active security settings.
* **Motherboard:** Access chipset details, BIOS manufacturer, release date, and specific board revision numbers.
* **Processor (CPU):** Microcode version, physical/logical core counts, L1/L2/L3 cache hierarchies, and real-time virtualization status.
* **Memory (RAM):** Slot-by-slot breakdown including part numbers, manufacturer, bank labels, voltage levels, and true operating frequency.
* **Graphics (GPU):** Full adapter string, VRAM capacity, driver date/version, and active monitor refresh rates/resolutions.
* **Storage:** Real-time health monitoring, partition styles (GPT/MBR), interface types (NVMe/SATA), and precise capacity reports.

### Peripherals & Connectivity
* **Network:** Dynamic monitoring of MAC addresses, internal IP addresses, network adapter speeds, and active connection status.
* **USB:** Lists all connected USB controllers and devices with detailed hardware IDs.
* **Sound:** Deep dive into audio controllers, driver versions, and active output/input devices.
* **Battery:** Full health report including design capacity vs. full charge capacity, cycle count, and wear level percentage.

### Software & Management
* **Installed Drivers:** Comprehensive listing of kernel and system drivers with provider and version details.
* **Installed Services:** View all background services, their current status (Running/Stopped), and startup types.
* **Installed Applications:** A complete list of software installed on the system, including hidden components.
* **Export Data:** Advanced reporting tool to save your entire system analysis as a clean, structured document.

---

## Advanced Tools Menu
Glow includes a built-in suite for system maintenance:
* **DISM & SFC Automation:** Automated system file repair and image health management.
* **Cache Cleanup:** Instantly clear system, browser, and temporary junk files.
* **Performance Benchmarks:** High-precision CPU, RAM, and Disk performance testing modules.
* **Screen Overlay:** Real-time system monitoring overlay for power users.
* **Network Suite:** DNS Test Tool and Network Troubleshooting for connectivity issues.
* **Security & Access:** Wi-Fi Password Showing Tool and System ID Generation.
* **Connectivity Tools:** Bluetooth Information utilities.
* **Monitor Test Suite:**
    * **Dead Pixel Test:** Identify faulty pixels across various color ranges.
    * **Dynamic Color Range:** Test your monitor's color depth and accuracy.
    * **Stuck Pixel Repair:** Advanced tool designed to revive unresponsive pixels.

---

## Getting Started

1.  Navigate to the **[Releases](https://github.com/turkaysoft/glow/releases)** page.
2.  Download the latest ZIP file.
3.  **Extract all files from the ZIP** (Important: Application requires all folder contents to run correctly).
4.  Launch the executable corresponding to your architecture:
    * `Glow_x64.exe`: For standard 64-bit Intel/AMD systems.
    * `Glow_arm64.exe`: For ARM-based devices like Surface Pro.

---

## Translation Support

* **Translation Support:** Community-driven localization via the official [Translation Guide](https://github.com/turkaysoft/glow/discussions/20).

---

## System Requirements

| Feature | Minimum Requirements | Recommended Requirements |
| :--- | :--- | :--- |
| **OS** | Windows 10 20H2 x64 | Windows 10 22H2 x64 |
| **CPU** | x64 or ARM64 | x64 or ARM64 |
| **RAM** | 75 MB Free RAM | 100 MB Free RAM |
| **.NET** | .NET Framework 4.8.1 | .NET Framework 4.8.1 |

---

## Shortcut Keys

| Shortcut | Action |
|--|--|
| `F1` | Light Theme |
| `F2` | Dark Theme |
| `F3` | System Theme |
| `F4` | Starting With: Windowed |
| `F5` | Starting With: Full Screen |
| `F6` | Hiding Mode: On |
| `F7` | Hiding Mode: Off |
| `F8` | Debug Mode: On |
| `F9` | Debug Mode: Off |
| `F10` | Open Debug Folder |
| `F11` | Check Updates |
| `F12` | About |
| `CTRL + Alt + D` | Donate Page |
| `CTRL + 1` | DISM and SFC Automation Tool |
| `CTRL + 2` | CPU Benchmark Tool |
| `CTRL + 3` | RAM Benchmark Tool |
| `CTRL + 4` | Disk Benchmark Tool |
| `CTRL + 5` | Cache Cleanup Tool |
| `CTRL + 6` | System ID Analysis Tool |
| `CTRL + 7` | Screen Overlay |
| `CTRL + Shift + B` | Bluetooth Information Tool |
| `CTRL + Shift + D` | DNS Test Tool |
| `CTRL + Shift + N` | Network Troubleshooting Tool |
| `CTRL + Shift + W` | Wi-Fi Password Showing Tool |
| `CTRL + Shift + M` | Dead Pixel Test |
| `CTRL + Alt + M` | Dynamic Color Range Test |
| `CTRL + Alt + Shift + M` | Stuck Pixel Repair Tool |

---

## Security

* **Zero Data Export Policy:** Your privacy is our priority; no data leaves your machine.
* **No Dependencies:** Developed entirely from scratch using its own source code, there are no risks from security vulnerabilities in third-party libraries.
* **Open Source:** All source code for the program is open and can be reviewed by anyone.

---

## License

This software is offered free of charge as part of the **Türkaysoft solutions package** and is protected under the [**MIT License**](https://github.com/turkaysoft/glow?tab=MIT-1-ov-file).
