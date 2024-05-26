// private void FindNearestCheckpoint()
// {
//     // Get the player's position
//     Vector3 playerPosition = Controller.transform.position;
//
//     Checkpoint nearestCheckpoint = Utils.FindMaxObject<Checkpoint>((currentMax, obj) =>
//     {
//         return Vector3.Distance(playerPosition, currentMax.transform.position) >
//                Vector3.Distance(playerPosition, obj.transform.position);
//     });
//
//     if (nearestCheckpoint.bIsActivated)
//     {
//         CurrentCheckpoint = nearestCheckpoint;
//     }
// }