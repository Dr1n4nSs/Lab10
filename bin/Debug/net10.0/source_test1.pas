program Summa; 
const
  MaxCount = 500;
  Multiplier = 1.5;
  DebugMode = true;
  LineBreak = 'n';
  AppTitle = 'Compiler v2';
  ValidStates = (off, loading, active, failure);
  WorkingHours = 9 .. 18;
var  
  idCode      : integer;
  weight      : real;
  isActive    : boolean;
  keySymbol   : char;
  username    : string;      
  logMessage  : string[100];  
  currentMode : enumerate (idle, processing, done);
  ageLimits   : range 18 .. 65;
begin   
  result := first + second; 
  s := 222;
  a := b + 5; 
 end. 