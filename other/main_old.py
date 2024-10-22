#!/usr/bin/env python3

import pygame
import socket

UDP_IP = "0.0.0.0"
UDP_PORT = 1214
SMOOTH = True
WIN_SIZE = 640


def main():
    pygame.init()
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    sock.bind((UDP_IP, UDP_PORT))

    win = pygame.display.set_mode((WIN_SIZE, WIN_SIZE))
    pygame.display.set_caption("Live View")
    run = True

    surface = pygame.Surface((8, 8))
    surface.fill((255, 0, 0))

    lookup_table = generate_lookup_table(color1=(0, 255, 0), color2=(255, 0, 0), color3=(0, 0, 255), max_range=4096)

    while run:
        data, addr = sock.recvfrom(128)
        if len(data) != 128:
            print("Data len Error")
            continue

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                run = False

        distance = convert_data(data)
        print(distance)

        for y in range(8):
            for x in range(8):
                index = x * 8 + y
                color = lookup_table[distance[index]]
                surface.set_at((x, y), color)

        if SMOOTH:
            pygame.transform.smoothscale(surface, (WIN_SIZE, WIN_SIZE), win)
        else:
            pygame.transform.scale(surface, (WIN_SIZE, WIN_SIZE), win)

        pygame.display.update()


def convert_data(input_data, max_range=4096):
    distance_list = []
    for i in range(0, len(input_data), 2):
        high_byte = input_data[i]
        low_byte = input_data[i + 1]
        distance = (high_byte << 8) | low_byte
        if distance > max_range:
            distance = max_range
        distance_list.append(distance)
    return distance_list


def interpolate_color(color_start, color_end, factor):
    """ Interpolates between two colors by a factor between 0 and 1. """
    return tuple(
        int(color_start[i] + (color_end[i] - color_start[i]) * factor) for i in range(3)
    )


def generate_lookup_table(color1=(255, 0, 0), color2=(0, 0, 255), color3=(0, 255, 0), max_range=4096):
    """ Generates a lookup table mapping values to colors based on the gradient. """
    lookup_table = {}

    for value in range(max_range + 1):
        normalized = value / max_range

        if normalized < 0.5:
            factor = normalized * 2
            color = interpolate_color(color1, color2, factor)
        else:
            factor = (normalized - 0.5) * 2
            color = interpolate_color(color2, color3, factor)

        lookup_table[value] = color

    return lookup_table


if __name__ == '__main__':
    main()
