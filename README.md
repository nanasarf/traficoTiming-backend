# traficoTiming-backend
## Offline heat replay API

Start the API with `dotnet run --project traficoTiming/TraficoTiming.Api --launch-profile http` (.NET 10). POST the companion frontend's heat.json manifest to `/api/timing/replays/evaluate`. This read-only endpoint requires no database and does not publish results. It maps the selected finish frames into the starter clock using two supplied calibration anchors per camera. It returns elapsed lower and upper bounds in microseconds, rounded outwards. A lane with missing evidence has null bounds. Absolute accuracy, calibration validity in the field, and finish identity require independent review.

Run `dotnet test traficoTiming/TraficoTiming.Tests` for the test suite.
