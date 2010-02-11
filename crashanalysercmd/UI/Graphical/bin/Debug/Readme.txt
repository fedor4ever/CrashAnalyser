Crash Analyser (c) 2008 Symbian Ltd
===================================

Written by Alex.Wilbur@symbian.com

Portions of this tool are copyright (c) their respective owners. 
See SupportingLibraries.txt for further information.


INSTALLATION INSTRUCTIONS
=========================

This tool requires the MicroSoft .NET v2.0 RunTime.

You can download this from MicroSoft's website:

http://www.microsoft.com/downloads/details.aspx?familyid=0856EACB-4362-4B0D-8EDD-AAB15C5E04F5&displaylang=en

CHANGES
=======

0.51
----
- Various mobile crash related fixes
- Minor tweaks to internal stack walking algorithms


0.50
----
- Implemented new stack walking algorithm, which can accurately decode function calls
  providing that a core rom image is supplied. The tool uses the arm instructions
  in conjunction with symbol data in order to accurately walk the function call 
  chain. Thanks to TomG for the implementation details!
- Totally re-wrote the symbolic configuration editor - you no longer have to supply
  core/rofs symbol files specifically. The tool will work out which is which.
- Added better XREF integration into the call stack viewer.
- Fixed some bugs in the symbol engine relating to labels overriding function addresses
- Fixed some bugs in the ROFS symbol engine that had crept in since 0.30
- Fixed a bug in raw stack data that resulted in dynamic codesegments not being correctly identified
- More standards compliant Core Dump support.


0.40
----
- Added early version of NICD viewer


0.31
----
- Bug fixes to engines
- Added support for new MC fields up to version 9 of the file format


0.30
----
- GUI fixes
- added SymbianWizardLib
- extended SymbianZipLib


0.27
----
- updated sharpziplib to latest version
- escape key no longer closes wizard
- added support for new mobile crash fields
- work around mobile crash sometimes capturing the wrong stack
- prevent zip files from containing zero-length symbol files
- better handling of exception types for D_EXC and MobileCrash files (CoreDump was okay)
- fixed some cosmetic UI issues


0.26
----
- better detection and handling of corrupt mobile crash files
- close core dump file handles after parsing is complete
- include offset information in register tooltips and clipboard operations
- save as zip feature for mobile crash files no longer includes 'tmp' files


0.25
----
- added better CPSR viewer
- fix for ELF core dump exception info (it was previously missing)
- put a work around in for a core dump string defect


0.24
----
- fix for string table note problem with ELF core dump


0.23
----
- better IOP with mobile crash, and some mobile crash-specific bug fixes


0.22
----
- bug fixes
- save symbols now only saves used symbol info


0.21
----
- fixed a bug relating to loading of ROFS symbol file symbols
- exception when parsing some symbol files (timing related)
- fixed mobile crash "extra information" inability to load XPTable dll
