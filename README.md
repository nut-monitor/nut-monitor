# nut-monitor

This is a small application that query a NUT server for UPS status, and take action when the UPS is on battery.

## Usage

`.\NutMonitor.exe --server [nut-server] --port [3493] --ups-name [ups-name] --ob-action "[command]"`

For example, the following command will query `nut-server.contoso.com` for the status of UPS `qnapups`, and run `shutdown /s` if the UPS is on battery.

`.\NutMonitor.exe --server nut-server.contoso.com --ups-name qnapups --ob-action "shutdown /s"`

To simulate a power outage, add `--dry-run` to run the command regardless of the battery status.

To use **nut-monitor** as a monitor, run `.\NutMonitor.exe` at short intervals (for example, one minute) with Windows Task Scheduler or cron.
