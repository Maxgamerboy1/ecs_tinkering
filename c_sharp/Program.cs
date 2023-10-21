// See https://aka.ms/new-console-template for more information
using DefaultEcs;
using DefaultEcs.Command;
using DefaultEcs.System;

Console.WriteLine("Hello, World!");
var world = new World();
var moveItemsSystem = new MoveItems(world);
setup_world(world);

while (true)
{
    moveItemsSystem.Update(1);
    Thread.Sleep(2000);
}

void setup_world(World world)
{
    var top_left = world.CreateEntity();
    top_left.Set(new Storage());
    top_left.Set(new InputConnection { dir = 0 });
    top_left.Set(new OutputConnection { dir = 270 });
    top_left.Set(new StorableItem(ItemType.Iron));
    top_left.Set(new Location(new Vec2 { x = 0, y = 0 }));

    var bottom_left = world.CreateEntity();
    bottom_left.Set(new Storage());
    bottom_left.Set(new InputConnection { dir = 90 });
    bottom_left.Set(new OutputConnection { dir = 0 });
    bottom_left.Set(new Location(new Vec2 { x = 0, y = 1 }));

    var bottom_right = world.CreateEntity();
    bottom_right.Set(new Storage());
    bottom_right.Set(new InputConnection { dir = 180 });
    bottom_right.Set(new OutputConnection { dir = 90 });
    bottom_right.Set(new Location(new Vec2 { x = 1, y = 1 }));
    bottom_right.Set(new StorableItem(ItemType.Coal));

    var top_right = world.CreateEntity();
    top_right.Set(new Storage());
    top_right.Set(new InputConnection { dir = 270 });
    top_right.Set(new OutputConnection { dir = 180 });
    top_right.Set(new Location(new Vec2 { x = 1, y = 0 }));

    // world.spawn((
    //     Storage,
    //     InputConnection::default(),
    //     OutputConnection::default(),
    //     Location(Vec2 { x: 0., y: 10. }),
    // ));
}

class MoveItems : AEntitySetSystem<int>
{
    private readonly EntitySet inputs;
    private readonly World world;

    public MoveItems(World world) : base(world.GetEntities().With<Storage>().With<OutputConnection>().AsSet())
    {
        inputs = world.GetEntities().With<InputConnection>().AsSet();
        this.world = world;
    }

    protected override void Update(int state, ReadOnlySpan<Entity> output_entities)
    {
        base.Update(state, output_entities);
        var recorder = new EntityCommandRecorder();

        foreach (var output_entity in output_entities)
        {
            var output_entity_record = recorder.Record(output_entity);
            foreach (var input_entity in inputs.GetEntities())
            {
                var input_entity_record = recorder.Record(input_entity);

                (StorableItem? stored_item, Location output_location, OutputConnection output_dir) = (output_entity.Has<StorableItem>() ? output_entity.Get<StorableItem>() : null, output_entity.Get<Location>(), output_entity.Get<OutputConnection>());
                var (input_location, input_dir) = (input_entity.Get<Location>(), input_entity.Get<InputConnection>());

                var x_dist = Math.Abs(output_location.Loc.x - input_location.Loc.x);
                var y_dist = Math.Abs(output_location.Loc.y - input_location.Loc.y);

                if (stored_item.HasValue
                    && input_entity != output_entity
                    && ((x_dist == 1 && y_dist == 0) || (y_dist == 1 && x_dist == 0))
                    && Math.Abs(output_dir.dir - input_dir.dir) == 180)
                {
                    Console.WriteLine($"Removing from: {output_entity}");
                    output_entity_record.Remove<StorableItem>();

                    Console.WriteLine($"Adding to: {input_entity}");
                    input_entity_record.Set(stored_item.Value);
                    break;
                }
            }
        }

        recorder.Execute();
    }
}

struct Storage { }
struct InputConnection
{
    public int dir { get; set; }
}
struct OutputConnection
{
    public int dir { get; set; }
}

enum ItemType
{
    Iron,
    Coal,
}

record struct StorableItem(ItemType Item);

record struct Location(Vec2 Loc);
struct Vec2
{
    public int x { get; set; }
    public int y { get; set; }
}