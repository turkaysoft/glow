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
* **Saving Data:** The ability to save all features offered in the interface as a **Text Document**, **HTML Document**, **Markdown Document** and **JSON Document**, if desired.
* **System Identity:** Unique device ID generation for hardware verification (Shortcut: `CTRL + 6`).
* **Modern UI:** Clean, intuitive interface compatible with Windows 11 design language, featuring Light, Dark, and System themes.
* **Multilingual:** It supports 16 different languages, primarily English. You can access the supported languages here: [Supported Languages](https://github.com/turkaysoft/glow/discussions/20)
* **Built-in Update Mechanism:** It features a built-in smart update mechanism developed specifically by **Türkaysoft**.

---

## Interface Preview

<img width="1010" height="633" alt="Glow UI" src="https://github.com/user-attachments/assets/e8b102dc-dada-46e3-83e8-551041997446" />

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
* **DISM and SFC Automation Tool:** Automated system file repair and image health management.
* **CPU Benchmark Tool:** CPU performance testing module.
* **RAM Benchmark Tool:** RAM performance testing module.
* **Disk Benchmark Tool:** Disk performance testing module.
* **Cache Cleanup Tool:** Instantly clear system and temporary junk files.
* **System ID Analysis Tool:** Detailed analysis for unique system identifiers.
* **Hardware Security Audit Tool:** Comprehensive security auditing for your system hardware.
* **Bluetooth Information Tool:** Detailed connectivity utilities for Bluetooth devices.
* **DNS Test Tool:** Network suite tool for analyzing and testing DNS performance.
* **Network Troubleshooting Tool:** Diagnostic suite for resolving connectivity issues.
* **Wi-Fi Password Showing Tool:** Instantly display saved Wi-Fi networks and security keys.
* **Port Scanner Tool:** Analyze TCP/UDP ports and network listeners, view connected processes and risk levels, and scan local or remote hosts.
* **Monitor Test Tools:**
    * **Dead Pixel Test:** Identify faulty pixels across various color ranges.
    * **Dynamic Color Range Test:** Test your monitor's color depth and accuracy.
    * **Stuck Pixel Repair Tool:** Advanced tool designed to revive unresponsive pixels.
 
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
| **OS** | Windows 10 22H2 x64 | Windows 11 25H2 x64 |
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
| `CTRL + 7` | Hardware Security Audit Tool |
| `CTRL + Shift + B` | Bluetooth Information Tool |
| `CTRL + Shift + D` | DNS Test Tool |
| `CTRL + Shift + N` | Network Troubleshooting Tool |
| `CTRL + Shift + W` | Wi-Fi Password Showing Tool |
| `CTRL + Shift + P` | Port Scanner Tool |
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


<details>
<summary><b>📝 Opis v Slovenščini</b></summary>

# Glow - Programska oprema za napredno sistemsko analizo

[![GitHub downloads](https://img.shields.io/github/downloads/turkaysoft/glow/total?style=flat&color=1a893c&label=Downloads)](https://github.com/turkaysoft/glow/releases)
[![GitHub stars](https://img.shields.io/github/stars/turkaysoft/glow?style=flat&color=0062cc&label=Stars)](https://github.com/turkaysoft/glow/stargazers)
[![GitHub release](https://img.shields.io/github/v/release/turkaysoft/glow?style=flat&color=5a32a3&label=Latest%20Release)](https://github.com/turkaysoft/glow/releases/latest)
[![Platform](https://img.shields.io/badge/platform-Windows-b31d28?style=flat&label=Platform)](https://github.com/turkaysoft/glow)

**Glow** je napredna programska oprema za analizo sistema, ki jo je razvil **Eray Türkay**. Zasnovana je za odkrivanje tehničnih podrobnosti, do katerih je s standardnimi orodji težko dostopati ali pa pogosto ostanejo povsem skrite. Od najglobljih ravni strojne arhitekture do zapletene strukture spremenljivk okolja programske opreme. Glow analizira vaš sistem z visoko natančnostjo.

**Z več kot 500 naprednimi funkcijami** in obsežnimi možnostmi analize program Glow preučuje sistemske komponente z več zornih kotov, s čimer razkriva ključne podatke, ki bi sicer ostali neopaženi, ter uporabnikom ponuja obsežne tehnične vpoglede.

S predstavitvijo vseh teh informacij na preprost, jasen in urejen način omogoča, da tudi uporabniki z omejenim računalniškim znanjem z lahkoto razumejo svoje sisteme in prek preglednega prikaza hitro dostopajo do podatkov, ki jih potrebujejo.

---

### Darujte
Ta projekt lahko podprete z donacijo, s čimer pripomorete k zagotavljanju njegove trajnosti in razvoju novih funkcij.

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-Donate-0a6628?style=flat&logo=buy-me-a-coffee&logoColor=white)](https://buymeacoffee.com/turkaysoft)

---

## Ključne značilnosti

* **Prednost zasebnosti:** Vaši podatki ostanejo v vaši napravi; nobeni podatki se ne prenašajo na zunanje strežnike.
* **Vrhunska zmogljivost:** Razvito izključno v okolju **C# in .NET Framework**, brez zunanjih knjižnic ali odvisnosti.
* **Prenosljivost:** Namestitev ni potrebna. Preprosto prenesite datoteko, razširite vsebino arhiva ZIP, izberite ustrezno arhitekturo in zaženite program.
* **Shranjevanje podatkov:** Možnost shranjevanja vseh informacij, prikazanih v vmesniku, v obliki **besedilnega dokumenta**, **dokumenta HTML**, **dokumenta Markdown** ali **dokumenta JSON**.
* **Identiteta sistema:** Generiranje edinstvenega ID-ja naprave za preverjanje strojne opreme (bližnjica: `CTRL + 6`).
* **Sodoben uporabniški vmesnik:** Čist in intuitiven vmesnik, skladen z oblikovnim slogom sistema Windows 11, ki ponuja svetlo, temno in sistemsko temo.
* **Večjezičnost:** Podpira 16 različnih jezikov (privzeti jezik je angleščina). Seznam podprtih jezikov si lahko ogledate tukaj: [Podprti jeziki](https://github.com/turkaysoft/glow/discussions/20)
* **Vgrajen mehanizem za posodabljanje:** Vključuje pameten mehanizem za posodabljanje, ki ga je posebej razvilo podjetje **Türkaysoft**.

---

## Predogled vmesnika

<img width="1010" height="633" alt="Glow UI" src="https://github.com/user-attachments/assets/e8b102dc-dada-46e3-83e8-551041997446" />

---

## Tehnični vpogledi in poglobljeni podatki

Glow analizira globoke sistemske ravni in zagotavlja podatke na profesionalni ravni v okviru specializiranih modulov – razkriva vam tisto, česar drugi ne morejo:

### Osnovne strojne komponente
* **Operacijski sistem:** Podrobne številke različic sistema Windows, čas namestitve, trenutni čas delovanja (»Up Time«) in aktivne varnostne nastavitve.
* **Matična plošča:** Podatki o naboru čipov (chipsetu), proizvajalcu BIOS-a, datumu izdaje in specifičnih številkah revizije plošče.
* **Procesor (CPU):** Različica mikrokode, število fizičnih in logičnih jeder, hierarhija predpomnilnika (L1/L2/L3) ter trenutno stanje virtualizacije.
* **Pomnilnik (RAM):** Podroben pregled po posameznih režah, vključno s kataloškimi številkami, proizvajalcem, oznakami pomnilniških bank, napetostmi in dejansko delovno frekvenco.
* **Grafična kartica (GPU):** Celoten naziv adapterja, kapaciteta video pomnilnika (VRAM), datum in različica gonilnika ter trenutne frekvence osveževanja in ločljivosti zaslonov.
* **Shranjevanje podatkov:** Spremljanje stanja delovanja v realnem času, vrste particij (GPT/MBR), vmesniki (NVMe/SATA) in natančni podatki o kapaciteti.

### Zunanje naprave in povezljivost
* **Omrežje:** Dinamično spremljanje naslovov MAC, notranjih naslovov IP, hitrosti omrežnih vmesnikov in stanja aktivnih povezav.
* **USB:** Seznam vseh povezanih krmilnikov in naprav USB s podrobnimi identifikatorji strojne opreme.
* **Zvok:** Podroben pregled zvočnih krmilnikov, različic gonilnikov ter aktivnih izhodnih in vhodnih naprav.
* **Baterija:** Celovito poročilo o stanju baterije, ki vključuje primerjavo projektirane kapacitete in dejanske kapacitete pri polni napolnjenosti, število ciklov ter odstotek obrabe.

### Programska oprema in upravljanje
* **Nameščeni gonilniki:** Celovit seznam gonilnikov jedra in sistemskih gonilnikov s podatki o ponudniku in različici.
* **Nameščene storitve:** Pregled vseh storitev, ki delujejo v ozadju, njihovega trenutnega stanja (deluje/ustavljeno) in načina zagona.
* **Nameščene aplikacije:** Popoln seznam programske opreme, nameščene v sistemu, vključno s skritimi komponentami.
* **Izvoz podatkov:** Napredno orodje za poročanje, ki omogoča shranjevanje celotne sistemske analize v obliki preglednega in strukturiranega dokumenta.

---

## Meni naprednih orodij

Glow vključuje vgrajen paket za vzdrževanje sistema:
* **Orodje za avtomatizacijo DISM in SFC:** Avtomatizirano popravilo sistemskih datotek in upravljanje zdravja slik.
* **Orodje za primerjavo CPU:** Modul za testiranje zmogljivosti CPU.
* **Orodje za primerjavo RAM:** Modul za testiranje zmogljivosti RAM.
* **Orodje za primerjavo diska:** Modul za testiranje zmogljivosti diska.
* **Orodje za čiščenje predpomnilnika:** Takojšnje čiščenje sistemskih in začasnih neželenih datotek.
* **Orodje za analizo sistemskega ID-ja:** Podrobna analiza edinstvenih sistemskih identifikatorjev.
* **Orodje za pregled varnosti strojne opreme:** Celovit pregled varnosti strojne opreme vašega sistema.
* **Orodje za informacije o Bluetooth:** Podrobna orodja za povezljivost za naprave Bluetooth.
* **Orodje za testiranje DNS:** Orodje za analizo in testiranje zmogljivosti DNS.
* **Orodje za odpravljanje težav z omrežjem:** Diagnostični paket za reševanje težav s povezljivostjo.
* **Orodje za prikaz gesel za Wi-Fi:** Takojšnje prikazovanje shranjenih omrežij Wi-Fi in varnostnih ključev.
* **Orodje za skeniranje vrat:** Analizira vrata TCP/UDP in omrežne poslušalce, si ogleda povezane procese in stopnje tveganja ter skenira lokalne ali oddaljene gostitelje.
* **Orodja za preizkušanje monitorjev:**
    * **Preizkus mrtvih slikovnih pik:** Odkrivanje okvarjenih slikovnih pik v različnih barvnih razponih.
    * **Preizkus dinamičnega barvnega razpona:** Preverjanje barvne globine in natančnosti vašega monitorja.
    * **Orodje za popravljanje zataknjenih pikslov:** Napredno orodje, namenjeno ponovni aktivaciji pikslov, ki ne odgovarjajo.
 
---

## Začetek

1.  Pomaknite se na **[Releases](https://github.com/turkaysoft/glow/releases)** stran.
2.  Prenesite najnovejšo ZIP datoteko.
3.  **Razširite vse datoteke iz datoteke ZIP** (Pomembno: za pravilno delovanje aplikacije je potrebna celotna vsebina mape).
4.  Zaženite izvedljivo datoteko, ki ustreza vaši arhitekturi:
    * `Glow_x64.exe`: Za standardne 64-bitne Intel/AMD sisteme.
    * `Glow_arm64.exe`: Za naprave na osnovi ARM, kot je Surface Pro.

---

## Podpora pri prevajanju

* **Podpora pri prevajanju:** Lokalizacija, ki jo vodi skupnost, prek uradnega [Vodiča za prevajanje](https://github.com/turkaysoft/glow/discussions/20).

---

## Sistemske zahteve

| Značilnosti | Minimalne zahteve | Priporočene zahteve |
| :--- | :--- | :--- |
| **OS** | Windows 10 22H2 x64 | Windows 11 25H2 x64 |
| **CPU** | x64 or ARM64 | x64 or ARM64 |
| **RAM** | 75 MB Free RAM | 100 MB Free RAM |
| **.NET** | .NET Framework 4.8.1 | .NET Framework 4.8.1 |

---

## Tipke za bližnjice

| Bližnjica | Ukrep |
|--|--|
| `F1` | Svetla tema |
| `F2` | Temna tema |
| `F3` | Sistemska tema |
| `F4` | Začetek: V oknu |
| `F5` | Začetek: Celoten zaslon |
| `F6` | Način skrivanja: Vklopljen |
| `F7` | Način skrivanja: Izklopljen |
| `F8` | Način odpravljanja napak: Vklopljen |
| `F9` | Način odpravljanja napak: Izklopljen |
| `F10` | Odpri mapo za odpravljanje napak |
| `F11` | Preveri posodobitve |
| `F12` | O programu |
| `CTRL + Alt + D` | Stran za donacije |
| `CTRL + 1` | Orodje za avtomatizacijo DISM in SFC |
| `CTRL + 2` | Orodje za merjenje zmogljivosti procesorja (CPU) |
| `CTRL + 3` | Orodje za merjenje zmogljivosti pomnilnika RAM |
| `CTRL + 4` | Orodje za merjenje zmogljivosti diska |
| `CTRL + 5` | Orodje za čiščenje predpomnilnika |
| `CTRL + 6` | Orodje za analizo sistemskega ID-ja |
| `CTRL + 7` | Orodje za pregled varnosti strojne opreme |
| `CTRL + Shift + B` | Orodje za informacije o Bluetoothu |
| `CTRL + Shift + D` | Orodje za testiranje DNS |
| `CTRL + Shift + N` | Orodje za odpravljanje težav z omrežjem |
| `CTRL + Shift + W` | Orodje za prikaz gesla za Wi-Fi |
| `CTRL + Shift + P` | Orodje za skeniranje vrat |
| `CTRL + Shift + M` | Test mrtvih pikslov |
| `CTRL + Alt + M` | Test dinamičnega barvnega obsega |
| `CTRL + Alt + Shift + M` | Orodje za popravljanje zataknjenih pikslov |

---

## Varnost

* **Politika neizvažanja podatkov:** Vaša zasebnost je naša prednostna naloga; nobeni podatki ne zapustijo vaše naprave.
* **Brez odvisnosti:** Program je v celoti razvit od začetka z uporabo lastne izvorne kode, zato ni tveganj zaradi varnostnih ranljivosti v knjižnicah tretjih oseb.
* **Odprta koda:** Celotna izvorna koda programa je odprta in jo lahko pregleda kdorkoli.

---

## Licenca

Ta programska oprema je na voljo brezplačno kot del **paketa rešitev Türkaysoft** in je zaščitena z [**MIT License**](https://github.com/turkaysoft/glow?tab=MIT-1-ov-file).

</details>
