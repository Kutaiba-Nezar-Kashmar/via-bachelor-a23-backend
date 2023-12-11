INSERT INTO achievement(id, name, description, reward, icon)
VALUES (0, 'Un Assigned', 'Default value', 0, ''),
       (1, 'Canary 1', 'Join in 5 music related events', 100, 'https://i.imgur.com/CaGThkj.png'),
       (2, 'Canary 2', 'Join in 20 music related events', 200, 'https://i.imgur.com/CaGThkj.png'),
       (3, 'Canary 3', 'Join in 50 music related events', 270, 'https://i.imgur.com/CaGThkj.png'),
       (4, 'Owl 1', 'Join in 5 educational or learning related events', 100, 'https://i.imgur.com/bhxs305.png'),
       (5, 'Owl 2', 'Join in 20 educational or learning related events', 200, 'https://i.imgur.com/bhxs305.png'),
       (6, 'Owl 3', 'Join in 50 educational or learning related related events', 270, 'https://i.imgur.com/bhxs305.png'),
       (7, 'Peacock 1', 'Join in 5 cultural or artistic related events', 100, 'https://i.imgur.com/MYjGti5.png'),
       (8, 'Peacock 2', 'Join in 20 cultural or artistic related events', 200, 'https://i.imgur.com/MYjGti5.png'),
       (9, 'Peacock 3', 'Join in 50 cultural or artistic related related events', 270, 'https://i.imgur.com/MYjGti5.png'),
       (10, 'Bear 1', 'Join in 5 food or drink related events', 100, 'https://i.imgur.com/NGm0qRJ.png'),
       (11, 'Bear 2', 'Join in 20 food or drink related events', 200, 'https://i.imgur.com/NGm0qRJ.png'),
       (12, 'Bear 3', 'Join in 50 food or drink related related events', 270, 'https://i.imgur.com/NGm0qRJ.png'),
       (13, 'Butterfly 1', 'Join in 5 social or community based related events', 100, 'https://i.imgur.com/DFepSA7.png'),
       (14, 'Butterfly 2', 'Join in 20 social or community related events', 200, 'https://i.imgur.com/DFepSA7.png'),
       (15, 'Butterfly 3', 'Join in 50 social or community related related events', 270, 'https://i.imgur.com/DFepSA7.png'),
       (16, 'Cheetah 1', 'Join in 5 sport or physically active events', 100, 'https://i.imgur.com/WHmQ8S8.png'),
       (17, 'Cheetah 2', 'Join in 20 sport or physically active events', 200, 'https://i.imgur.com/WHmQ8S8.png'),
       (18, 'Cheetah 3', 'Join in 50 sport or physically active related events', 270, 'https://i.imgur.com/WHmQ8S8.png'),
       (19, 'Monkey 1', 'Join in 5 recreational or hobby based events', 100, 'https://i.imgur.com/jIt6H5B.png'),
       (20, 'Monkey 2', 'Join in 20 recreational or hobby based events', 200, 'https://i.imgur.com/jIt6H5B.png'),
       (21, 'Monkey 3', 'Join in 50 recreational or hobby based related events', 270, 'https://i.imgur.com/jIt6H5B.png'),
       (22, 'New comer', 'Fill out the interest survey', 0, 'https://i.imgur.com/8SgrQAL.png') ON CONFLICT DO NOTHING ;
      