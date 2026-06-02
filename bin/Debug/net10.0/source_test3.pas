program FunctionErrorDemo;

var
    globalX : integer;

function (x : integer) : boolean;
begin
    Func1 := true;
end;

function CalcSum(a : integer; b : integer : integer;
begin
    CalcSum := a + b;
end;

function ProcessData(val : real) : boolean
begin
    ProcessData := false;
end;

function LocalScopeError : integer;
var
    temp : integer;
const
    LocalConst = 10;
begin
    LocalScopeError := LocalConst;
end;

function DropSemicolon : integer;
begin
    DropSemicolon := 1;
end

begin
    globalX := 10;
end.