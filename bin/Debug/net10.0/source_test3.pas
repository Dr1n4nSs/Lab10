program SemanticMaster;

const
    MaxCount = 100;

var
    globalInt  : integer;
    globalReal : real;
    globalStr  : string;

var
    globalInt : real; 

function DuplicateParam(x : integer; x : real) : integer;
begin
    DuplicateParam := 1;
end;

function DuplicateLocal : integer;
var
    loc : integer;
    loc : real; 
begin
    DuplicateLocal := 1;
end;

function UnknownIdentifiers : integer;
begin
    UnknownIdentifiers := undefinedVariable + 5; 
end;

function AssignToConst : integer;
begin
    MaxCount := 200; 
    AssignToConst := 1;
end;

function BadAssignment : integer;
begin
    globalInt := 'Hello'; 
    BadAssignment := 1;
end;

function BadExpression : integer;
begin
    globalReal := globalStr * 2.5; 
    BadExpression := 1;
end;

begin
    globalInt := 10;
end.