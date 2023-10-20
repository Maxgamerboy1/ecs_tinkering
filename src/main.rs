use std::{ops::Mul, thread::sleep, time::Duration};

use bevy_ecs::prelude::*;

#[derive(Component)]
struct Storage;
#[derive(Component, Default)]
struct InputConnection {
    dir: i32,
}
#[derive(Component, Default)]
struct OutputConnection {
    dir: i32,
}

#[derive(Clone, Copy)]
enum ItemType {
    Iron,
}

#[derive(Component, Clone, Copy)]
struct StorableItem(ItemType);

#[derive(Component)]
struct Location(Vec2);
#[derive(Default, PartialEq, Clone, Copy)]
struct Vec2 {
    x: f32,
    y: f32,
}

fn main() -> ! {
    println!("Hello, world!");
    let mut world = World::new();

    world.spawn((
        Storage,
        InputConnection { dir: 0 },
        OutputConnection { dir: 270 },
        StorableItem(ItemType::Iron),
        Location(Vec2 { x: 0., y: 0. }),
    ));
    world.spawn((
        Storage,
        InputConnection { dir: 90 },
        OutputConnection { dir: 0 },
        Location(Vec2 { x: 0., y: 1. }),
    ));
    world.spawn((
        Storage,
        InputConnection { dir: 180 },
        OutputConnection { dir: 90 },
        Location(Vec2 { x: 1., y: 1. }),
    ));
    world.spawn((
        Storage,
        InputConnection { dir: 270 },
        OutputConnection { dir: 180 },
        Location(Vec2 { x: 1., y: 0. }),
    ));

    world.spawn((
        Storage,
        InputConnection::default(),
        OutputConnection::default(),
        Location(Vec2 { x: 0., y: 10. }),
    ));

    let mut schedule = Schedule::default();
    schedule.add_systems(move_items);

    loop {
        schedule.run(&mut world);
        sleep(Duration::from_secs(2));
    }
}

fn move_items(
    mut commands: Commands,
    storage_to_output: Query<
        (Entity, &StorableItem, &Location, &OutputConnection),
        (With<Storage>, With<OutputConnection>),
    >,
    inputs: Query<(Entity, &Location, &InputConnection), With<InputConnection>>,
) {
    // TODO: see if we can instead get entities straight up rather than iterating all of them (nested for loop is the badger to target)
    for (output_entity, stored_item, output_location, output_dir) in storage_to_output.iter() {
        for (input_entity, input_location, input_dir) in inputs.iter() {
            let x_dist = (output_location.0.x - input_location.0.x).abs();
            let y_dist = (output_location.0.y - input_location.0.y).abs();

            if input_entity != output_entity // output entity aint the same as input entity
                && ((x_dist == 1. && y_dist == 0.) || (y_dist == 1. && x_dist == 0.)) //entities are next to each other
                && (output_dir.dir - input_dir.dir).abs() == 180
            //Input and Output are along the same axis
            {
                // retrieve the StoredItem associated with the Storage entity we're looping
                println!("Removing from: {:?}", output_entity);
                commands.entity(output_entity).remove::<StorableItem>();

                println!("Adding to: {:?}", input_entity);
                commands.entity(input_entity).insert(*stored_item);
                break;
            }
        }
    }
}
