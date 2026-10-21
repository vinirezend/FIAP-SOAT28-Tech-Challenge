// Os cenários compartilham o mesmo banco (limpo pelo Respawn a cada cenário),
// portanto não podem rodar em paralelo.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
