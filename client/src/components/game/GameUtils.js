export const getGameStateText = (state) => {
    switch (state) {
        case 0: return "Ожидание противника";
        case 1: return "Расстановка / Готовность";
        case 2: return "Игра идет";
        case 3: return "Игра завершена";
        default: return "Неизвестный статус";
    }
}; 