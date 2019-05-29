Note that there are different options in configuration to build project.
For debug use:
x86 for Any CPU programs
x64 for x64 based systems
DLL will be created in $(SolutionDir)\lsd\$(Platform)\$(Configuration)\ 
You'll need to add DLL's in lsd.net projects (x86 and x64) folders by yourself, or just change output directory in configuration manager (Project Properties\Configuration properties\General)
Note that you need both x64 and x86 to work on Any CPU