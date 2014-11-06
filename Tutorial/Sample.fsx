(**
Calculating factorial
---------------------

 * Run: `factorial 10000L`
 * Repeat: 1000

*)
let rec factorial x = 
  if x = 0L then 1L 
  else x * (factorial (x - 1L))
(**

Working with large arrays
-------------------------

 * Run: `test ()`
 * Repeat: 10

*)
let test() =
  let arr = Array.init 1000000 (fun n -> float n)
  arr |> Array.map (fun v -> v ** 2.0)
(**

String concatenation
--------------------

 * Run: `replicate 1000 "hi"`
 * Repeat: 200

*)
let replicate times str =
  Seq.init times (fun _ -> str)
  |> Seq.fold (+) ""

