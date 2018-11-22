# Purpose

This folder holds Azure Web App job for monitoring and autoscaling Azure Agents Virtual Machines Scale Set.

## Implementation

Azure Web app job checks Azure DevOps account specified and monitors queue. As soon as there is waiting job - new VM in VMSS shall be started.
If queue is empty and we have more than 1 VM running in VMSS - extra VMs shall be stopped. If there is 1 VM in VMSS and it is running for 1 hour without a jobs - it shall be deprovisioned as well.

Job shall check maximum amount of possible private agents in account and ensure that VMSS have this amount of VMs provisioned (faster startup times, though we'll pay extra for disk drives).

Job shall log start/stop attempts in external storage for future ML model training. 

Job is written on C#, as it is easier than Powershell for me :)