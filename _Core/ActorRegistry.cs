using System.Collections.Generic;

public static class ActorRegistry
{
    public static List<Actor> Actors { get;  set; } = new List<Actor>();
    public static Actor PlayerActor { get;  set; }
    
    public static Dictionary<string,List<Actor>> ActorGroups = new Dictionary<string, List<Actor>>();

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

    public static void RegisterActorToGroup(Actor actor, string group)
    {
        if(!ActorGroups.ContainsKey(group))
        {
            ActorGroups[group] = new List<Actor>();
        }
        if(!ActorGroups[group].Contains(actor))
        {
            ActorGroups[group].Add(actor);
        }
    }
    
    public static void UnregisterActorFromGroup(Actor actor,string group)
    {
        if(ActorGroups.ContainsKey(group))
        {
            if(!ActorGroups[group].Contains(actor)) return;
            ActorGroups[group].Remove(actor);
        }
    }

    public static Actor GetFirstActorWithGroupTag(string tag)
    {
        if(ActorGroups.ContainsKey(tag))
        {
            return ActorGroups[tag][0];
        }
        return null;
    }
}