$ErrorActionPreference = 'Stop'
$taskSource = [IO.File]::ReadAllText((Join-Path $PSScriptRoot '../Runtime/ITGT/DesignationCycle.cs'))
$taskChecks = @'
namespace PhantasmsArsenal.ITGT {
public static class CycleChecks {
 public static string Run() {
  var cycle = new DesignationCycle<string>();
  var marks = new System.Collections.Generic.List<string>{"1","2","3","4","5","6"};
  string[] expected = {"1","2","3","4","5","6","1","2","3","4"};
  foreach (var value in expected) {
   var captured = cycle.Current("A", marks);
   if (captured != value) throw new System.Exception("10 releases / 6 marks");
   cycle.Advance("A", marks, captured);
  }
  if (cycle.Current("B", marks) != "1") throw new System.Exception("Independent types");
  var held = cycle.Current("A", marks);
  for (int i=0; i<10; i++)
   if (cycle.Current("A", marks) != held) throw new System.Exception("Preview advanced queue");
  cycle.Advance("A", marks, "1");
  if (cycle.Current("A", marks) != held) throw new System.Exception("Stale commit advanced queue");
  cycle.Remove(held, marks); marks.Remove(held);
  if (cycle.Current("A", marks) != "6") throw new System.Exception("Deleted next mark successor");
  cycle.Advance("A", marks, "6");
  if (cycle.Current("A", marks) != "1") throw new System.Exception("Wrap after deletion");
  cycle.Reset("A");
  if (cycle.Current("A", marks) != "1") throw new System.Exception("Reset");
  cycle.Clear(); marks.Clear();
  if (cycle.Current("A", marks) != null) throw new System.Exception("Empty plan");
  marks.Add("new");
  if (cycle.Current("A", marks) != "new") throw new System.Exception("New plan");
  return "PASS: sequence, independent types, previews, stale commits, deletion, wrap, reset, empty/new plans.";
 }
}}
'@
Add-Type -TypeDefinition ($taskSource + $taskChecks)
[PhantasmsArsenal.ITGT.CycleChecks]::Run()
