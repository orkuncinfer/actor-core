using System.Collections.Generic;

public static class ActorRegistry
{
    public static List<Actor> Actors { get;  set; } = new List<Actor>();
    public static Actor PlayerActor { get;  set; }

    public static void RegisterActor(Actor actor)
    {
        if(Actors.Contains(actor)) return;
        Actors.Add(actor);

        if (actor.ContainsTag("Player"))
        {
            PlayerActor = actor;
        }
    }

    public static void UnregisterActor(Actor actor)
    {
        Actors.Remove(actor);
    }
}